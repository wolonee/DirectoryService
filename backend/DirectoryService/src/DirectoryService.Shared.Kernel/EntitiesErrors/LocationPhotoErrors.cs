using DirectoryService.Shared.Errors;

namespace DirectoryService.Shared.EntitiesErrors;

public static class LocationPhotoErrors
{
    public static Error AssetIdRequired() =>
        Error.Validation(
            "location.photo.asset.id.required",
            "Photo asset id is required.",
            "AssetId");

    public static Error InvalidContentType(string? contentType) =>
        Error.Validation(
            "location.photo.content.type.invalid",
            $"Content type '{contentType}' is not a supported image type.",
            "ContentType");

    public static Error InvalidAttachedAt() =>
        Error.Validation(
            "location.photo.attached.at.invalid",
            "Photo attachment time is invalid.",
            "AttachedAt");

    public static Error AssetNotReady(Guid assetId, string status) =>
        Error.Conflict(
            "location.photo.asset.not.ready",
            $"Asset {assetId} is not ready. Current status: {status}.");

    public static Error WrongTarget(Guid assetId, Guid locationId) =>
        Error.Conflict(
            "location.photo.asset.wrong.target",
            $"Asset {assetId} is not assigned to location {locationId}.");

    public static Error WrongAssetType(Guid assetId, string assetType) =>
        Error.Conflict(
            "location.photo.asset.wrong.type",
            $"Asset {assetId} has unsupported asset type '{assetType}'.");

    public static Error WrongUsageType(Guid assetId, string usageType) =>
        Error.Conflict(
            "location.photo.asset.wrong.usage",
            $"Asset {assetId} has unsupported usage type '{usageType}'.");

    public static Error WrongContentType(Guid assetId, string contentType) =>
        Error.Conflict(
            "location.photo.asset.wrong.content.type",
            $"Asset {assetId} has unsupported content type '{contentType}'.");
}
