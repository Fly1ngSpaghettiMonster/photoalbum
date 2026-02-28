namespace PhotoAlbum.Web.Data;

public class Photo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime UploadedAtUtc { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public ApplicationUser? OwnerUser { get; set; }
    public string BlobName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}
