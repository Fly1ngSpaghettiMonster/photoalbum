using Microsoft.AspNetCore.Hosting;

namespace PhotoAlbum.Web.Services;

public class LocalPhotoStorage(IWebHostEnvironment environment) : IPhotoStorage
{
    private const string UploadsFolderName = "uploads";

    public async Task<string> SaveAsync(Stream content, string fileExtension, CancellationToken cancellationToken = default)
    {
        var uploadsPath = EnsureUploadsFolder();
        var safeExtension = NormalizeExtension(fileExtension);
        var blobName = $"{Guid.NewGuid():N}{safeExtension}";
        var filePath = Path.Combine(uploadsPath, blobName);

        await using var fileStream = File.Create(filePath);
        await content.CopyToAsync(fileStream, cancellationToken);

        return blobName;
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        var uploadsPath = EnsureUploadsFolder();
        var filePath = Path.Combine(uploadsPath, blobName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return Task.CompletedTask;
    }

    public string GetPublicUrl(string blobName)
    {
        return $"/{UploadsFolderName}/{blobName}";
    }

    private string EnsureUploadsFolder()
    {
        var webRoot = environment.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot = Path.Combine(environment.ContentRootPath, "wwwroot");
        }

        var uploadsPath = Path.Combine(webRoot, UploadsFolderName);
        Directory.CreateDirectory(uploadsPath);

        return uploadsPath;
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
