using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts;

namespace DirectoryService.Application.Locations.Commands.Photo;

public static class LocationPhotoPolicy
{
    public static UnitResult<Error> Validate(
        GetMediaAssetResponse asset,
        Guid expectedLocationId)
    {
        if (!string.Equals(asset.Status, "ready", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(asset.ContentUrl))
        {
            return LocationPhotoErrors.AssetNotReady(asset.FileId, asset.Status);
        }

        if (!string.Equals(asset.OwnerContext, "location", StringComparison.OrdinalIgnoreCase)
            || asset.EntityId != expectedLocationId)
        {
            return LocationPhotoErrors.WrongTarget(asset.FileId, expectedLocationId);
        }

        if (!string.Equals(asset.AssetType, "preview", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoErrors.WrongAssetType(asset.FileId, asset.AssetType);

        if (!string.Equals(asset.UsageType, "location_photo", StringComparison.OrdinalIgnoreCase))
            return LocationPhotoErrors.WrongUsageType(asset.FileId, asset.UsageType);

        if (string.IsNullOrWhiteSpace(asset.ContentType)
            || !asset.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return LocationPhotoErrors.WrongContentType(asset.FileId, asset.ContentType);
        }

        return UnitResult.Success<Error>();
    }
}
