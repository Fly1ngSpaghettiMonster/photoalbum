using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace PhotoAlbum.Web.Services;

public class AzureBlobPhotoStorage : IPhotoStorage
{
    private readonly BlobContainerClient blobContainerClient;
    private readonly string publicBaseUrl;

    public AzureBlobPhotoStorage(IConfiguration configuration)
    {
        var options = configuration.GetSection(StorageOptions.SectionName).Get<StorageOptions>() ?? new StorageOptions();

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            throw new InvalidOperationException("Storage:ConnectionString is required when Storage:Provider is AzureBlob.");
        }

        blobContainerClient = new BlobContainerClient(options.ConnectionString, options.ContainerName);
        blobContainerClient.CreateIfNotExists(PublicAccessType.Blob);
        publicBaseUrl = options.PublicBaseUrl?.TrimEnd('/') ?? string.Empty;
    }

    public async Task<string> SaveAsync(Stream content, string fileExtension, string contentType, CancellationToken cancellationToken = default)
    {
        var safeExtension = NormalizeExtension(fileExtension);
        var blobName = $"{Guid.NewGuid():N}{safeExtension}";
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType
            }
        };

        await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

        return blobName;
    }

    public async Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var blobClient = blobContainerClient.GetBlobClient(blobName);

        try
        {
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (RequestFailedException)
        {
            // no-op for MVP cleanup consistency
        }
    }

    public string GetPublicUrl(string blobName)
    {
        if (!string.IsNullOrWhiteSpace(publicBaseUrl))
        {
            return $"{publicBaseUrl}/{blobName}";
        }

        return blobContainerClient.GetBlobClient(blobName).Uri.ToString();
    }

    private static string NormalizeExtension(string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(fileExtension))
        {
            return ".bin";
        }

        return fileExtension.StartsWith('.') ? fileExtension : $".{fileExtension}";
    }
}
