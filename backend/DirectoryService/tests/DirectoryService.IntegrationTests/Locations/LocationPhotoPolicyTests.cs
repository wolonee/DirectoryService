using DirectoryService.Application.Locations.Commands.Photo;
using FileService.Contracts;

namespace DirectoryService.IntegrationTests.Locations;

public class LocationPhotoPolicyTests
{
    [Fact]
    public void Validate_ValidLocationPhoto_ReturnsSuccess()
    {
        Guid locationId = Guid.CreateVersion7();
        GetMediaAssetResponse asset = CreateAsset(locationId);

        var result = LocationPhotoPolicy.Validate(asset, locationId);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData("uploading", "location", "preview", "location_photo", "image/webp", "https://file.test", "location.photo.asset.not.ready")]
    [InlineData("ready", "course", "preview", "location_photo", "image/webp", "https://file.test", "location.photo.asset.wrong.target")]
    [InlineData("ready", "location", "video", "location_photo", "image/webp", "https://file.test", "location.photo.asset.wrong.type")]
    [InlineData("ready", "location", "preview", "course_cover", "image/webp", "https://file.test", "location.photo.asset.wrong.usage")]
    [InlineData("ready", "location", "preview", "location_photo", "video/mp4", "https://file.test", "location.photo.asset.wrong.content.type")]
    [InlineData("ready", "location", "preview", "location_photo", "image/webp", null, "location.photo.asset.not.ready")]
    public void Validate_InvalidAsset_ReturnsExpectedError(
        string status,
        string context,
        string assetType,
        string usageType,
        string contentType,
        string? contentUrl,
        string expectedCode)
    {
        Guid locationId = Guid.CreateVersion7();
        GetMediaAssetResponse asset = CreateAsset(
            locationId,
            status,
            context,
            assetType,
            usageType,
            contentType,
            contentUrl);

        var result = LocationPhotoPolicy.Validate(asset, locationId);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }

    private static GetMediaAssetResponse CreateAsset(
        Guid locationId,
        string status = "ready",
        string context = "location",
        string assetType = "preview",
        string usageType = "location_photo",
        string contentType = "image/webp",
        string? contentUrl = "https://file.test") =>
        new(
            Guid.CreateVersion7(),
            locationId,
            context,
            status,
            assetType,
            contentType,
            usageType,
            1_024,
            null,
            contentUrl);
}
