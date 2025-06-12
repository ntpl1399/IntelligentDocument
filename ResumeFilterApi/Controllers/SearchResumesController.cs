using Microsoft.AspNetCore.Mvc;
using ResumeFilterApi.Services;

namespace ResumeFilterApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchResumesController : ControllerBase
    {
        private readonly ResumeSearchService _resumeSearchService;

        public SearchResumesController(ResumeSearchService resumeSearchService)
        {
            _resumeSearchService = resumeSearchService;
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int top = 20)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query cannot be empty.");

            var results = await _resumeSearchService.SearchResumesAsync(query, top);
            return Ok(results);
        }
    }
}
