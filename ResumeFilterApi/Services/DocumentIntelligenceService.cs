using Azure;
using Azure.AI.FormRecognizer.DocumentAnalysis;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentAnalysisClient _client;

    private readonly DocumentModelAdministrationClient _client1;

    private readonly string ? sasToken;

    public DocumentIntelligenceService(IConfiguration config)
    {
        var endpoint = new Uri(config["DocumentIntelligence:Endpoint"]);
        var key = new AzureKeyCredential(config["DocumentIntelligence:ApiKey"]);
        sasToken = config["sasToken"];

        // Initialize the correct client types
        _client = new DocumentAnalysisClient(endpoint, key);
        _client1 = new DocumentModelAdministrationClient(endpoint, key);
    }

    // Pseudocode plan:
    // 1. Check if the blobUrl is correct and accessible from Azure Form Recognizer (public or with correct SAS token).
    // 2. Ensure the SAS token is valid and not expired.
    // 3. Log or throw a more descriptive error if the URL is not accessible.
    // 4. Optionally, add a pre-check to verify the blob exists before calling AnalyzeDocumentFromUriAsync.

    public async Task<ResumeDocument> ExtractResumeInsightsAsync(string blobUrl)
    {
        //string? sasToken = config["sasToken"];
        //"sv=2024-11-04&ss=bfqt&srt=sco&sp=rwdlacupiytfx&se=2025-11-29T18:34:15Z&st=2025-09-06T10:19:15Z&spr=https&sig=jSC9DGvY0cOg3JKGMPHOb4PRyKX9JxQS2%2FDAbDSWocs%3D";
        // Append SAS token if provided and not already present
        if (!string.IsNullOrWhiteSpace(sasToken) && !blobUrl.Contains("sig="))
        {
            var separator = blobUrl.Contains('?') ? "&" : "?";
            blobUrl = $"{blobUrl}{separator}{sasToken.TrimStart('?')}";
        }

        var fileUri = new Uri(blobUrl);

        try
        {
            //Optionally: Pre - check if the blob exists and is accessible
            using (var httpClient = new HttpClient())
            {
                var headRequest = new HttpRequestMessage(HttpMethod.Head, fileUri);
                var headResponse = await httpClient.SendAsync(headRequest);
                if (!headResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Blob at {blobUrl} is not accessible (HTTP {(int)headResponse.StatusCode}).");
                }
            }

            using var httpClient1 = new HttpClient();
            var stream = await httpClient1.GetStreamAsync(fileUri);


            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reset position to the beginning


            



            await foreach (var model in _client1.GetDocumentModelsAsync())
            {
                Console.WriteLine($"Model ID: {model.ModelId}");
            }



            var operation = await _client.AnalyzeDocumentAsync(WaitUntil.Completed, "ResumesModel", memoryStream);




            //AnalyzeDocumentOperation operation = await _client.AnalyzeDocumentFromUriAsync(WaitUntil.Completed, "prebuilt-resume", uri);
            AnalyzeResult result = operation.Value;


            //var operation = await _client.AnalyzeDocumentFromUriAsync(
            //    WaitUntil.Completed,
            //    "prebuilt-resume",
            //    uri);

            //var result = operation.Value;

            var doc = result.Documents.FirstOrDefault();
            if (doc == null)
                throw new Exception("No document was returned by the analysis.");

            //string GetFieldContent(string fieldName) =>
            //    doc.Fields.TryGetValue(fieldName, out var field) && !string.IsNullOrWhiteSpace(field?.Content)
            //        ? field.Content
            //        : throw new Exception($"Field '{fieldName}' not found or empty in the analyzed document.");

            string GetFieldContent(string fieldName) =>
            doc.Fields.TryGetValue(fieldName, out var field) ? field?.Content : throw new Exception($"Field '{fieldName}' not found in the analyzed document.");
                    
                    
            string GetSkills()
            {
                if (doc.Fields.TryGetValue("Skills", out var skillsField) && !string.IsNullOrWhiteSpace(skillsField?.Content))
                    return string.Join(",", skillsField.Content.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)));
                throw new Exception("Field 'Skills' not found or empty in the analyzed document.");
            }

            return new ResumeDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = GetFieldContent("Name"),
                Email = string.Empty, //GetFieldContent("email"),
                Skills = GetSkills(),
                Education = GetFieldContent("Education"),
                Experience = GetFieldContent("Experience"),
                ResumeUrl = blobUrl
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new Exception($"Resource not found or not accessible at the provided URL: {blobUrl}. Ensure the URL is correct and accessible by Azure Form Recognizer.", ex);
        }
    }
}
