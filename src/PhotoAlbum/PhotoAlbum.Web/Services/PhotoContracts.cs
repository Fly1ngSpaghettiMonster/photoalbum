namespace PhotoAlbum.Web.Services;

public enum PhotoSortBy
{
    Date,
    Name
}

public sealed record PhotoListItem(
    Guid Id,
    string Name,
    DateTime UploadedAtUtc,
    string ImageUrl,
    bool CanDelete);

public sealed record PhotoDetailItem(
    Guid Id,
    string Name,
    DateTime UploadedAtUtc,
    string ImageUrl,
    bool CanDelete);

public sealed record UploadPhotoRequest(
    string Name,
    string FileName,
    string ContentType,
    long SizeBytes,
    Stream Content);

public sealed record OperationResult(bool Succeeded, string? Error)
{
    public static OperationResult Success() => new(true, null);

    public static OperationResult Failure(string error) => new(false, error);
}
