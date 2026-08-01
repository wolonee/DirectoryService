namespace FileService.Core.Models;

public sealed record DeleteObjectResult(
    string? DeleteMarker,
    string? VersionId);
