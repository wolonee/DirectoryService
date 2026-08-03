using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations.ValueObjects;

public sealed record LocationPhoto
{
    public Guid AssetId { get; }
    public string ContentType { get; }
    public DateTime AttachedAt { get; }

    private LocationPhoto(
        Guid assetId,
        string contentType,
        DateTime attachedAt)
    {
        AssetId = assetId;
        ContentType = contentType;
        AttachedAt = attachedAt;
    }

    public static Result<LocationPhoto, Error> Create(
        Guid assetId,
        string contentType,
        DateTime attachedAt)
    {
        if (assetId == Guid.Empty)
            return LocationPhotoErrors.AssetIdRequired();

        if (string.IsNullOrWhiteSpace(contentType)
            || !contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return LocationPhotoErrors.InvalidContentType(contentType);
        }

        if (attachedAt == default || attachedAt > DateTime.UtcNow)
            return LocationPhotoErrors.InvalidAttachedAt();
        
        return new LocationPhoto(assetId, contentType.Trim(), attachedAt);
    }
}
