using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public sealed record StorageReference
{
    public StorageKey Key { get; }

    public long Size { get; }

    public string ContentType { get; }

    public string? ETag { get; }

    public string? Checksum { get; }

    public DateTime? LastModified { get; }

    private StorageReference(
        StorageKey key,
        long size,
        string contentType,
        string? eTag,
        string? checksum,
        DateTime? lastModified)
    {
        Key = key;
        Size = size;
        ContentType = contentType;
        ETag = eTag;
        Checksum = checksum;
        LastModified = lastModified;
    }

    public static Result<StorageReference, Error> Create(
        StorageKey key,
        long size,
        string contentType,
        string? eTag,
        string? checksum,
        DateTime? lastModified)
    {
        if (key == StorageKey.None)
            return GeneralErrors.ValueIsInvalid(nameof(key));

        if (size <= 0)
            return GeneralErrors.ValueIsInvalid(nameof(size));

        if (string.IsNullOrWhiteSpace(contentType))
            return GeneralErrors.ValueIsInvalid(nameof(contentType));

        return new StorageReference(key, size, contentType, eTag, checksum, lastModified);
    }
}
