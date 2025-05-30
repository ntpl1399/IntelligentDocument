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

    // Add error handling to check for 404 (Not Found) errors when calling the Azure Form Recognizer API.
    public async Task<ResumeDocument> ExtractResumeInsightsAsync(string blobUrl)
    {
        var uri = new Uri(blobUrl);

        try
        {
            var operation = await _client.AnalyzeDocumentFromUriAsync(
                WaitUntil.Completed,
                "prebuilt-resume",
                uri);

            var result = operation.Value;

            return new ResumeDocument
            {
                Id = Guid.NewGuid().ToString(),
                Name = result.Documents[0].Fields["name"]?.Content,
                Email = result.Documents[0].Fields["email"]?.Content,
                Skills = result.Documents[0].Fields["skills"]?.Content?.Split(",").Select(s => s.Trim()).ToList(),
                Education = result.Documents[0].Fields["education"]?.Content,
                Experience = result.Documents[0].Fields["workExperience"]?.Content,
                ResumeUrl = blobUrl
            };
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            throw new Exception($"Resource not found at the provided URL: {blobUrl}. Please verify the URL and try again.", ex);
        }
    }
}
