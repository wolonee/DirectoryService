namespace FileService.Contracts.Features.Simple.GetMediaAssets;

public sealed record GetMediaAssetDto(
    Guid Id,
    string Status,
    string ContentType,
    string? ContentUrl);