using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using ResumeFilterApi.Models;


namespace ResumeFilterApi.Services
{
    public class ResumeSearchService
    {
        private readonly SearchClient _searchClient;

        public ResumeSearchService(IConfiguration configuration)
        {
            var endpoint = new Uri(configuration["AzureSearch:Endpoint"]);
            var credential = new AzureKeyCredential(configuration["AzureSearch:ApiKey"]);
            var indexName = configuration["AzureSearch:IndexName"];

            _searchClient = new SearchClient(endpoint, indexName, credential);
        }

        public async Task<List<ResumeSearchResult>> SearchResumesAsync(string query, int top = 20)
        {
            var options = new SearchOptions
            {
                Size = top,
                IncludeTotalCount = true,
                Select = { "id", "name", "email", "skills", "experience", "education", "resumeUrl" }
            };

            var results = new List<ResumeSearchResult>();
            var response = await _searchClient.SearchAsync<ResumeSearchResult>(query, options);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                if (result.Document != null)
                    results.Add(result.Document);
            }

            return results;
        }
    }
}


