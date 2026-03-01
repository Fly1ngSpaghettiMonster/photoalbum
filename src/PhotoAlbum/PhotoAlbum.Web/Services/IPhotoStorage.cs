namespace PhotoAlbum.Web.Services;

public interface IPhotoStorage
{
    Task<string> SaveAsync(Stream content, string fileExtension, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
    string GetPublicUrl(string blobName);
}
