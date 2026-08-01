namespace FileService.Contracts;

public sealed record GetMediaAssetResponse(
    Guid FileId,
    Guid EntityId,
    string OwnerContext,
    string Status,
    string AssetType,
    string ContentType,
    string UsageType,
    long Size,
    ObjectMetadataDto? Storage,
    string? ContentUrl);