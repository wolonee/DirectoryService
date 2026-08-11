namespace FileService.Contracts.Shared;

public sealed record ObjectMetadataDto(
    long ContentLength,
    string? ContentType,
    string? ETag,
    string? Checksum,
    DateTime? LastModified);
