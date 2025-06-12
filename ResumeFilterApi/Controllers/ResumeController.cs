using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ResumeController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly IDocumentIntelligenceService _docIntelService;
    private readonly IAzureSearchService _searchService;

    public ResumeController(
        IBlobStorageService blobStorageService,
        IDocumentIntelligenceService docIntelService,
        IAzureSearchService searchService)
    {
        _blobStorageService = blobStorageService;
        _docIntelService = docIntelService;
        _searchService = searchService;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadResume(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        // 1. Upload to Azure Blob Storage
        string blobUrl = await _blobStorageService.UploadResumeAsync(file);


        // 2. Extract data using Azure Document Intelligence
        var extractedData = await _docIntelService.ExtractResumeInsightsAsync(blobUrl);

        // 3. Store structured data into Azure AI Search

        await _searchService.IndexResumeAsync(extractedData);

        return Ok(new { message = "Resume processed successfully", blobUrl });
    }


}
