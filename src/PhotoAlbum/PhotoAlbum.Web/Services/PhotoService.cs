using Microsoft.EntityFrameworkCore;
using PhotoAlbum.Web.Data;

namespace PhotoAlbum.Web.Services;

public class PhotoService(ApplicationDbContext dbContext, IPhotoStorage photoStorage)
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public async Task<IReadOnlyList<PhotoListItem>> GetListAsync(string? currentUserId, PhotoSortBy sortBy, bool ascending, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return [];
        }

        IQueryable<Photo> query = dbContext.Photos
            .AsNoTracking()
            .Where(p => p.OwnerUserId == currentUserId);

        query = sortBy switch
        {
            PhotoSortBy.Name => ascending
                ? query.OrderBy(p => p.Name).ThenByDescending(p => p.UploadedAtUtc)
                : query.OrderByDescending(p => p.Name).ThenByDescending(p => p.UploadedAtUtc),
            _ => ascending
                ? query.OrderBy(p => p.UploadedAtUtc)
                : query.OrderByDescending(p => p.UploadedAtUtc)
        };

        var rows = await query
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.UploadedAtUtc,
                p.BlobName,
                p.OwnerUserId
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(p => new PhotoListItem(
                p.Id,
                p.Name,
                p.UploadedAtUtc,
                photoStorage.GetPublicUrl(p.BlobName),
                currentUserId is not null && p.OwnerUserId == currentUserId))
            .ToList();
    }

    public async Task<PhotoDetailItem?> GetByIdAsync(Guid id, string? currentUserId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return null;
        }

        var photo = await dbContext.Photos
            .AsNoTracking()
            .Where(p => p.Id == id && p.OwnerUserId == currentUserId)
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.UploadedAtUtc,
                p.BlobName,
                p.OwnerUserId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (photo is null)
        {
            return null;
        }

        return new PhotoDetailItem(
            photo.Id,
            photo.Name,
            photo.UploadedAtUtc,
            photoStorage.GetPublicUrl(photo.BlobName),
            currentUserId is not null && photo.OwnerUserId == currentUserId);
    }

    public async Task<OperationResult> UploadAsync(UploadPhotoRequest request, string ownerUserId, CancellationToken cancellationToken = default)
    {
        var validationError = ValidateUploadRequest(request);
        if (validationError is not null)
        {
            return OperationResult.Failure(validationError);
        }

        var extension = Path.GetExtension(request.FileName);
        var blobName = await photoStorage.SaveAsync(request.Content, extension, request.ContentType, cancellationToken);

        var entity = new Photo
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            UploadedAtUtc = DateTime.UtcNow,
            OwnerUserId = ownerUserId,
            BlobName = blobName,
            ContentType = request.ContentType,
            SizeBytes = request.SizeBytes
        };

        dbContext.Photos.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return OperationResult.Success();
    }

    public async Task<OperationResult> DeleteAsync(Guid id, string ownerUserId, CancellationToken cancellationToken = default)
    {
        var photo = await dbContext.Photos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (photo is null)
        {
            return OperationResult.Failure("Photo not found.");
        }

        if (!string.Equals(photo.OwnerUserId, ownerUserId, StringComparison.Ordinal))
        {
            return OperationResult.Failure("You can only delete your own photos.");
        }

        dbContext.Photos.Remove(photo);
        await dbContext.SaveChangesAsync(cancellationToken);
        await photoStorage.DeleteAsync(photo.BlobName, cancellationToken);

        return OperationResult.Success();
    }

    private static string? ValidateUploadRequest(UploadPhotoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Photo name is required.";
        }

        if (request.Name.Trim().Length > 40)
        {
            return "Photo name must be at most 40 characters.";
        }

        if (request.SizeBytes <= 0)
        {
            return "Uploaded file is empty.";
        }

        if (request.SizeBytes > MaxUploadBytes)
        {
            return "File size exceeds 10 MB limit.";
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            return "Unsupported file type. Use JPEG, PNG, or WEBP.";
        }

        return null;
    }
}
