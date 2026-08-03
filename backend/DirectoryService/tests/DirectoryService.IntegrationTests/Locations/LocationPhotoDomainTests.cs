using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObjects;

namespace DirectoryService.IntegrationTests.Locations;

public class LocationPhotoDomainTests
{
    [Fact]
    public void AttachReplaceRemovePhoto_ValidFlow_ChangesPhoto()
    {
        // Arrange
        Location location = CreateLocation();
        LocationPhoto firstPhoto = CreatePhoto(Guid.CreateVersion7());
        LocationPhoto secondPhoto = CreatePhoto(Guid.CreateVersion7());

        // Act
        var attachResult = location.AttachPhoto(firstPhoto);
        var replaceResult = location.ReplacePhoto(secondPhoto);
        var removeResult = location.RemovePhoto();

        // Assert
        Assert.True(attachResult.IsSuccess);
        Assert.True(replaceResult.IsSuccess);
        Assert.True(removeResult.IsSuccess);
        Assert.Null(location.Photo);
    }

    [Fact]
    public void PhotoOperations_WithInvalidState_ReturnConflicts()
    {
        // Arrange
        Location location = CreateLocation();
        LocationPhoto photo = CreatePhoto(Guid.CreateVersion7());

        // Act
        var replaceWithoutPhoto = location.ReplacePhoto(photo);
        var removeWithoutPhoto = location.RemovePhoto();
        _ = location.AttachPhoto(photo);
        var duplicateAttach = location.AttachPhoto(photo);
        var replaceSamePhoto = location.ReplacePhoto(photo);

        // Assert
        Assert.True(replaceWithoutPhoto.IsFailure);
        Assert.True(removeWithoutPhoto.IsFailure);
        Assert.True(duplicateAttach.IsFailure);
        Assert.True(replaceSamePhoto.IsFailure);
    }

    [Theory]
    [InlineData("image/webp", true)]
    [InlineData("video/mp4", false)]
    [InlineData("", false)]
    public void LocationPhoto_Create_ValidatesImageContentType(string contentType, bool shouldSucceed)
    {
        var result = LocationPhoto.Create(Guid.CreateVersion7(), contentType, DateTime.UtcNow);

        Assert.Equal(shouldSucceed, result.IsSuccess);
    }

    [Fact]
    public void LocationPhoto_Create_WithEmptyAssetId_ReturnsValidationError()
    {
        var result = LocationPhoto.Create(Guid.Empty, "image/webp", DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("location.photo.asset.id.required", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void LocationPhoto_Create_WithInvalidAttachedAt_ReturnsValidationError(int timeType)
    {
        DateTime attachedAt = timeType == 0
            ? default
            : DateTime.UtcNow.AddMinutes(1);

        var result = LocationPhoto.Create(Guid.CreateVersion7(), "image/webp", attachedAt);

        Assert.True(result.IsFailure);
        Assert.Equal("location.photo.attached.at.invalid", result.Error.Code);
    }

    private static Location CreateLocation() =>
        Location.Create(
            LocationAddress.Create("Main street 1", "Moscow", "Russia").Value,
            LocationName.Create("Main office").Value,
            LocationTimeZone.Create("Europe/Moscow").Value).Value;

    private static LocationPhoto CreatePhoto(Guid assetId) =>
        LocationPhoto.Create(assetId, "image/webp", DateTime.UtcNow).Value;
}
