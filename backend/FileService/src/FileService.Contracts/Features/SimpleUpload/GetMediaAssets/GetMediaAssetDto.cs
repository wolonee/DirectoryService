namespace FileService.Contracts;

public sealed record GetMediaAssetDto(
    Guid Id,
    string Status,
    string ContentType,
    string? ContentUrl);