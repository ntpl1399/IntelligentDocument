using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

public class AzureSearchService : IAzureSearchService
{
    private readonly SearchClient _searchClient;

    public AzureSearchService(IConfiguration config)
    {
        _searchClient = new SearchClient(
            new Uri(config["AzureSearch:Endpoint"]),
            "resumes-index",
            new AzureKeyCredential(config["AzureSearch:ApiKey"]));
    }

    public async Task IndexResumeAsync(ResumeDocument resume)
    {

        var batch = IndexDocumentsBatch.Create(
         IndexDocumentsAction.Upload(resume)
         );

        try
        {
            await _searchClient.IndexDocumentsAsync(batch);
            Console.WriteLine($"Indexed resume: {resume.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to index resume: {ex.Message}");
        }

    }
}
