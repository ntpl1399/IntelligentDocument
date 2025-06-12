using Azure.AI.FormRecognizer.DocumentAnalysis;
using Azure;

public class DocumentIntelligenceService : IDocumentIntelligenceService
{
    private readonly DocumentAnalysisClient _client;

    public DocumentIntelligenceService(IConfiguration config)
    {
        var endpoint = new Uri(config["DocumentIntelligence:Endpoint"]);
        var key = new AzureKeyCredential(config["DocumentIntelligence:ApiKey"]);
        _client = new DocumentAnalysisClient(endpoint, key);
    }

    // Pseudocode plan:
    // 1. Check if the blobUrl is correct and accessible from Azure Form Recognizer (public or with correct SAS token).
    // 2. Ensure the SAS token is valid and not expired.
    // 3. Log or throw a more descriptive error if the URL is not accessible.
    // 4. Optionally, add a pre-check to verify the blob exists before calling AnalyzeDocumentFromUriAsync.

    public async Task<ResumeDocument> ExtractResumeInsightsAsync(string blobUrl)
    {
        string? sasToken = "sp=r&st=2025-06-11T02:15:37Z&se=2025-06-30T10:15:37Z&spr=https&sv=2024-11-04&sr=c&sig=vLuLeuZJIV8XXdapSQqShQtUbhUznMsJ9X3jx9hLeDQ%3D";
        // Append SAS token if provided and not already present
        if (!string.IsNullOrWhiteSpace(sasToken) && !blobUrl.Contains("sig="))
        {
            var separator = blobUrl.Contains('?') ? "&" : "?";
            blobUrl = $"{blobUrl}{separator}{sasToken.TrimStart('?')}";
        }

        var uri = new Uri(blobUrl);

        try
        {
            //Optionally: Pre - check if the blob exists and is accessible
            using (var httpClient = new HttpClient())
            {
                var headRequest = new HttpRequestMessage(HttpMethod.Head, uri);
                var headResponse = await httpClient.SendAsync(headRequest);
                if (!headResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Blob at {blobUrl} is not accessible (HTTP {(int)headResponse.StatusCode}).");
                }
            }

            var operation = await _client.AnalyzeDocumentFromUriAsync(
                WaitUntil.Completed,
                "prebuilt-resume",
                uri);

            var result = operation.Value;

            var doc = result.Documents.FirstOrDefault();
            if (doc == null)
                throw new Exception("No document was returned by the analysis.");

            string GetFieldContent(string fieldName) =>
                doc.Fields.TryGetValue(fieldName, out var field) && !string.IsNullOrWhiteSpace(field?.Content)
                    ? field.Content
                    : throw new Exception($"Field '{fieldName}' not found or empty in the analyzed document.");

            List<string> GetSkills()
            {
                if (doc.Fields.TryGetValue("skills", out var skillsField) && !string.IsNullOrWhiteSpace(skillsField?.Content))
                    return skillsField.Content.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
                throw new Exception("Field 'skills' not found or empty in the analyzed document.");
            }

            return new ResumeDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = GetFieldContent("name"),
                Email = GetFieldContent("email"),
                Skills = GetSkills(),
                Education = GetFieldContent("education"),
                Experience = GetFieldContent("workExperience"),
                ResumeUrl = blobUrl
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new Exception($"Resource not found or not accessible at the provided URL: {blobUrl}. Ensure the URL is correct and accessible by Azure Form Recognizer.", ex);
        }
    }
}
