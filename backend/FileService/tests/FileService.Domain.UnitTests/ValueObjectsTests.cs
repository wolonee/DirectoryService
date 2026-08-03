using FileService.Domain;

namespace FileService.Domain.UnitTests;

public class ValueObjectsTests
{
    [Fact]
    public void FileName_Create_WithValidFile_ReturnsNameAndExtension()
    {
        var result = FileName.Create("lesson.MP4");

        Assert.True(result.IsSuccess);
        Assert.Equal("lesson.MP4", result.Value.Name);
        Assert.Equal("mp4", result.Value.Extension);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("video")]
    [InlineData("video.")]
    public void FileName_Create_WithInvalidFileName_ReturnsFailure(string fileName)
    {
        var result = FileName.Create(fileName);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void FileName_Create_WithTooLongName_ReturnsFailure()
    {
        var result = FileName.Create($"{new string('a', FileName.MAX_LENGTH)}.mp4");

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("video/mp4", MediaType.VIDEO)]
    [InlineData("image/png", MediaType.IMAGE)]
    [InlineData("audio/mpeg", MediaType.AUDIO)]
    public void ContentType_Create_WithMimeType_ReturnsCategory(string value, MediaType category)
    {
        var result = ContentType.Create(value);

        Assert.True(result.IsSuccess);
        Assert.Equal(category, result.Value.Category);
    }

    [Theory]
    [InlineData("")]
    [InlineData("video")]
    [InlineData("image png")]
    public void ContentType_Create_WithInvalidMimeType_ReturnsFailure(string value)
    {
        var result = ContentType.Create(value);

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData("lesson_video", MediaUsage.LESSON_VIDEO)]
    [InlineData("COURSE_COVER", MediaUsage.COURSE_COVER)]
    [InlineData("company_presentation", MediaUsage.COMPANY_PRESENTATION)]
    [InlineData(" location_photo ", MediaUsage.LOCATION_PHOTO)]
    public void MediaUsage_ToMediaUsage_WithValidValue_ReturnsEnum(string value, MediaUsage expected)
    {
        var result = value.ToMediaUsage();

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown_usage")]
    public void MediaUsage_ToMediaUsage_WithInvalidValue_ReturnsFailure(string value)
    {
        var result = value.ToMediaUsage();

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void StorageKey_Create_NormalizesSafeKey()
    {
        var result = StorageKey.Create(" videos ", " raw//lessons ", " file-1 ");

        Assert.True(result.IsSuccess);
        Assert.Equal("videos", result.Value.Bucket);
        Assert.Equal("raw/lessons", result.Value.Prefix);
        Assert.Equal("file-1", result.Value.Key);
        Assert.Equal("videos/raw/lessons/file-1", result.Value.FullPath);
    }

    [Theory]
    [InlineData("..")]
    [InlineData("../private")]
    [InlineData("raw/file")]
    [InlineData(@"raw\file")]
    public void StorageKey_Create_WithUnsafeKey_ReturnsFailure(string key)
    {
        var result = StorageKey.Create("videos", "raw", key);

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void StorageKey_Create_WithPathTraversalInPrefix_ReturnsFailure()
    {
        var result = StorageKey.Create("videos", "raw/../../private", "file-1");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void StorageKey_AppendSegment_CreatesNestedPath()
    {
        StorageKey root = StorageKey.Create("videos", "hls", "abc-123").Value;

        var result = root.AppendSegment("master.m3u8");

        Assert.True(result.IsSuccess);
        Assert.Equal("videos/hls/abc-123/master.m3u8", result.Value.FullPath);
    }

    [Fact]
    public void StorageKey_None_IsEmptyAndCannotAppendSegment()
    {
        Assert.Equal(string.Empty, StorageKey.None.FullPath);

        var result = StorageKey.None.AppendSegment("master.m3u8");

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MediaOwner_Create_NormalizesAllowedContext()
    {
        Guid entityId = Guid.CreateVersion7();

        var result = MediaOwner.Create(" LESSON ", entityId, Guid.CreateVersion7());

        Assert.True(result.IsSuccess);
        Assert.Equal("lesson", result.Value.Context);
        Assert.Equal(entityId, result.Value.EntityId);
    }

    [Fact]
    public void MediaOwner_ForLocation_CreatesLocationOwner()
    {
        Guid locationId = Guid.CreateVersion7();
        Guid uploaderId = Guid.CreateVersion7();

        var result = MediaOwner.ForLocation(locationId, uploaderId);

        Assert.True(result.IsSuccess);
        Assert.Equal("location", result.Value.Context);
        Assert.Equal(locationId, result.Value.EntityId);
        Assert.Equal(uploaderId, result.Value.UploaderId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("unknown")]
    public void MediaOwner_Create_WithInvalidContext_ReturnsFailure(string context)
    {
        var result = MediaOwner.Create(context, Guid.CreateVersion7(), Guid.CreateVersion7());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void MediaOwner_Create_WithEmptyEntityId_ReturnsFailure()
    {
        var result = MediaOwner.Create("lesson", Guid.Empty, Guid.CreateVersion7());

        Assert.True(result.IsFailure);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MediaData_Create_WithInvalidSize_ReturnsFailure(long size)
    {
        FileName fileName = FileName.Create("lesson.mp4").Value;
        ContentType contentType = ContentType.Create("video/mp4").Value;

        var result = MediaData.Create(fileName, contentType, size);

        Assert.True(result.IsFailure);
    }
}
