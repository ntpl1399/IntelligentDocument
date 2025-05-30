public interface IDocumentIntelligenceService
{
    Task<ResumeDocument> ExtractResumeInsightsAsync(string blobUrl);
}