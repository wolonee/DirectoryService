using FileService.Domain;

namespace FileService.Domain.UnitTests;

public class StorageReferenceTests
{
    [Fact]
    public void Create_WithValidMetadata_ReturnsStorageReference()
    {
        StorageKey key = StorageKey.Create("files", "raw", "document.pdf").Value;
        DateTime lastModified = new(2026, 7, 24, 10, 0, 0, DateTimeKind.Utc);

        var result = StorageReference.Create(
            key,
            1_024,
            "application/pdf",
            "etag-value",
            "checksum-value",
            lastModified);

        Assert.True(result.IsSuccess);
        Assert.Equal(key, result.Value.Key);
        Assert.Equal(1_024, result.Value.Size);
        Assert.Equal("application/pdf", result.Value.ContentType);
        Assert.Equal("etag-value", result.Value.ETag);
        Assert.Equal("checksum-value", result.Value.Checksum);
        Assert.Equal(lastModified, result.Value.LastModified);
    }

    [Fact]
    public void Create_WithEmptyKey_ReturnsFailure()
    {
        var result = StorageReference.Create(
            StorageKey.None,
            1_024,
            "application/pdf",
            null,
            null,
            null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithZeroSize_ReturnsFailure()
    {
        StorageKey key = StorageKey.Create("files", "raw", "document.pdf").Value;

        var result = StorageReference.Create(
            key,
            0,
            "application/pdf",
            null,
            null,
            null);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void Create_WithEmptyContentType_ReturnsFailure()
    {
        StorageKey key = StorageKey.Create("files", "raw", "document.pdf").Value;

        var result = StorageReference.Create(
            key,
            1_024,
            string.Empty,
            null,
            null,
            null);

        Assert.True(result.IsFailure);
    }
}
