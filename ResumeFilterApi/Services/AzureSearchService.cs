using Azure.Search.Documents;
using Azure;

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
        await _searchClient.UploadDocumentsAsync(new[] { resume });
    }
}
