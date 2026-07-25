namespace FileService.Contracts;

public sealed record DeleteObjectResponseDto(
    string? DeleteMarker,
    string? VersionId);
