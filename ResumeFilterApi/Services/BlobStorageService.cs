using Azure.Storage.Blobs;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    public BlobStorageService(IConfiguration config)
    {
        var blobServiceClient = new BlobServiceClient(config["Blob:ConnectionString"]);
        _containerClient = blobServiceClient.GetBlobContainerClient("resumes");
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> UploadResumeAsync(IFormFile file)
    {
        var blobClient = _containerClient.GetBlobClient(Guid.NewGuid() + "-" + file.FileName);

        using var stream = file.OpenReadStream();
        await blobClient.UploadAsync(stream, overwrite: true);

        return blobClient.Uri.ToString();
    }
}
