using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Infrastructure.Postgres.Configurations;
using FileService.Infrastructure.Postgres.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FileService.Domain.UnitTests;

public class StorageKeyPersistenceTests
{
    [Fact]
    public void StorageKeyConverter_RoundTrip_PreservesStorageKey()
    {
        StorageKey source = StorageKey.Create(
            "videos",
            "raw/lessons",
            "lesson.mp4").Value;
        var converter = new StorageKeyConverter();

        string storedValue = (string)converter.ConvertToProvider(source)!;
        StorageKey restored = (StorageKey)converter.ConvertFromProvider(storedValue)!;

        Assert.Equal(source, restored);
        Assert.Equal(source.FullPath, restored.FullPath);
    }

    [Fact]
    public void StorageKeyConverter_RoundTrip_PreservesNoneValue()
    {
        var converter = new StorageKeyConverter();

        string storedValue = (string)converter.ConvertToProvider(StorageKey.None)!;
        StorageKey restored = (StorageKey)converter.ConvertFromProvider(storedValue)!;

        Assert.Equal(StorageKey.None, restored);
        Assert.Equal(string.Empty, restored.FullPath);
    }

    [Fact]
    public void FileServiceModel_MapsStorageKeysAsJsonbWithIndexes()
    {
        using var dbContext = new FileServiceDbContext("Host=localhost;Database=model_check");
        IEntityType mediaAsset = dbContext.Model.FindEntityType(typeof(MediaAsset))!;
        IEntityType videoAsset = dbContext.Model.FindEntityType(typeof(VideoAsset))!;

        AssertJsonbStorageKey(mediaAsset, nameof(MediaAsset.RawKey));
        AssertJsonbStorageKey(mediaAsset, nameof(MediaAsset.FinalKey));
        AssertJsonbStorageKey(videoAsset, nameof(VideoAsset.HlsRootKey));

        Assert.Contains(mediaAsset.GetIndexes(), index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(MediaAsset.RawKey));
        Assert.Contains(mediaAsset.GetIndexes(), index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(MediaAsset.FinalKey));
        Assert.Contains(videoAsset.GetIndexes(), index =>
            index.Properties.Count == 1 &&
            index.Properties[0].Name == nameof(VideoAsset.HlsRootKey));
    }

    private static void AssertJsonbStorageKey(IEntityType entityType, string propertyName)
    {
        IProperty property = entityType.FindProperty(propertyName)!;

        Assert.Equal("jsonb", property.GetColumnType());
        Assert.IsType<StorageKeyConverter>(property.GetValueConverter());
    }
}
