using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using TalentMindAI.Application.Interfaces;
using TalentMindAI.Infrastructure.Configuration;

namespace TalentMindAI.Infrastructure.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobStorageOptions _options;

    public BlobStorageService(IOptions<BlobStorageOptions> options)
    {
        _options = options.Value;
    }

    public async Task<string> UploadAsync(string containerName, string blobName, Stream content, string contentType, CancellationToken ct = default)
    {
        var serviceClient = new BlobServiceClient(_options.ConnectionString);
        var containerClient = serviceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync(cancellationToken: ct);

        var blobClient = containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, new Azure.Storage.Blobs.Models.BlobHttpHeaders { ContentType = contentType }, cancellationToken: ct);

        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string blobUri, CancellationToken ct = default)
    {
        var blobClient = new BlobClient(new Uri(blobUri), null);
        var response = await blobClient.DownloadStreamingAsync(cancellationToken: ct);
        return response.Value.Content;
    }
}
