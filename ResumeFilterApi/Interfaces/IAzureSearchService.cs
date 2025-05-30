public interface IAzureSearchService
{
    Task IndexResumeAsync(ResumeDocument resume);
}