namespace PhotoAlbum.Web.Services;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Provider { get; set; } = "Local";
    public string ContainerName { get; set; } = "photos";
    public string ConnectionString { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
}
