using FileService.Contracts.Shared;

namespace FileService.Contracts.Features.Simple.GetMediaAsset;

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