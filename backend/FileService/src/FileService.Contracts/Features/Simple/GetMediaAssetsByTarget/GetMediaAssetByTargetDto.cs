namespace FileService.Contracts.Features.Simple.GetMediaAssetsByTarget;

public sealed record GetMediaAssetByTargetDto(
    Guid FileId,
    Guid EntityId,
    string OwnerContext,
    string Status,
    string ContentType,
    string? ContentUrl);