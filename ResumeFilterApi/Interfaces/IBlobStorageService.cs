public interface IBlobStorageService
{
    Task<string> UploadResumeAsync(IFormFile file);
}