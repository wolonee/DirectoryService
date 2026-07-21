using FileService.Domain;
using FileService.Domain.Assets;

namespace FileService.Domain.UnitTests;

public class AssetPoliciesTests
{
    [Fact]
    public void VideoAsset_CreateForUpload_WithValidVideo_CreatesUploadingVideo()
    {
        MediaData mediaData = CreateMediaData("lesson.mp4", "video/mp4", 1_024);

        var result = VideoAsset.CreateForUpload(
            Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, CreateOwner());

        Assert.True(result.IsSuccess);
        Assert.Equal(AssetType.VIDEO, result.Value.AssetType);
        Assert.Equal(MediaStatus.UPLOADING, result.Value.Status);
        Assert.StartsWith("videos/raw/", result.Value.RawKey.FullPath);
        Assert.StartsWith("videos/hls/", result.Value.HlsRootKey.FullPath);
    }

    [Fact]
    public void VideoAsset_CreateForUpload_WithImageContentType_ReturnsFailure()
    {
        MediaData mediaData = CreateMediaData("lesson.mp4", "image/png", 1_024);

        var result = VideoAsset.CreateForUpload(
            Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, CreateOwner());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PreviewAsset_CreateForUpload_WithValidImage_CreatesUploadingPreview()
    {
        MediaData mediaData = CreateMediaData("cover.webp", "image/webp", 1_024);

        var result = PreviewAsset.CreateForUpload(
            Guid.CreateVersion7(), mediaData, MediaUsage.COURSE_COVER, CreateOwner());

        Assert.True(result.IsSuccess);
        Assert.Equal(AssetType.PREVIEW, result.Value.AssetType);
        Assert.Equal(MediaStatus.UPLOADING, result.Value.Status);
        Assert.Equal("preview", result.Value.RawKey.Bucket);
    }

    [Fact]
    public void PreviewAsset_CreateForUpload_WithVideoExtension_ReturnsFailure()
    {
        MediaData mediaData = CreateMediaData("cover.mp4", "image/png", 1_024);

        var result = PreviewAsset.CreateForUpload(
            Guid.CreateVersion7(), mediaData, MediaUsage.COURSE_COVER, CreateOwner());

        Assert.True(result.IsFailure);
    }

    [Fact]
    public void PreviewAsset_CompleteUpload_MarksReadyWithRawKey()
    {
        PreviewAsset asset = PreviewAsset.CreateForUpload(
            Guid.CreateVersion7(),
            CreateMediaData("cover.webp", "image/webp", 1_024),
            MediaUsage.COURSE_COVER,
            CreateOwner()).Value;
        DateTime timestamp = new(2026, 7, 20, 13, 0, 0, DateTimeKind.Utc);

        var result = asset.CompleteUpload(timestamp);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.Equal(asset.RawKey, asset.FinalKey);
        Assert.Equal(timestamp, asset.UpdatedAt);
    }

    [Fact]
    public void VideoAsset_Validate_WithTooLargeFile_ReturnsFailure()
    {
        MediaData mediaData = CreateMediaData("lesson.mp4", "video/mp4", VideoAsset.MAX_SIZE + 1);

        var result = VideoAsset.ValidateForUpload(mediaData);

        Assert.True(result.IsFailure);
    }

    private static MediaData CreateMediaData(string fileName, string contentType, long size)
    {
        FileName name = FileName.Create(fileName).Value;
        ContentType type = ContentType.Create(contentType).Value;
        return MediaData.Create(name, type, size, 1).Value;
    }

    private static MediaOwner CreateOwner() => MediaOwner.ForLesson(Guid.CreateVersion7()).Value;
}
