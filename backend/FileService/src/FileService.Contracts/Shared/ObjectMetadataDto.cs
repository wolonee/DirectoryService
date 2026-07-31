namespace FileService.Contracts;

public sealed record ObjectMetadataDto(
    long ContentLength,
    string? ContentType,
    string? ETag,
    string? Checksum,
    DateTime? LastModified);
