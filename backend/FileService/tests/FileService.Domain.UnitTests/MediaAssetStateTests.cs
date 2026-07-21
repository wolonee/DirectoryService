using FileService.Domain;
using FileService.Domain.Assets;

namespace FileService.Domain.UnitTests;

public class MediaAssetStateTests
{
    [Fact]
    public void MarkUploaded_ThenMarkReady_ChangesStatusThroughAllowedTransitions()
    {
        VideoAsset asset = CreateAsset();
        DateTime uploadedAt = new(2026, 7, 20, 10, 0, 0, DateTimeKind.Utc);
        DateTime readyAt = uploadedAt.AddMinutes(5);
        StorageKey finalKey = StorageKey.Create("videos", "hls", "master.m3u8").Value;

        var uploadedResult = asset.MarkUploaded(uploadedAt);
        var readyResult = asset.MarkReady(finalKey, readyAt);

        Assert.True(uploadedResult.IsSuccess);
        Assert.True(readyResult.IsSuccess);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.Equal(readyAt, asset.UpdatedAt);
        Assert.Equal(finalKey, asset.FinalKey);
    }

    [Fact]
    public void CompleteProcessing_CreatesMasterPlaylistKeyAndMarksReady()
    {
        VideoAsset asset = CreateAsset();
        DateTime timestamp = new(2026, 7, 20, 12, 0, 0, DateTimeKind.Utc);
        asset.MarkUploaded(timestamp.AddMinutes(-1));

        var result = asset.CompleteProcessing(timestamp);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.Equal(
            $"videos/hls/{asset.Id}/{VideoAsset.MASTER_PLAYLIST_NAME}",
            asset.FinalKey.FullPath);
        Assert.Equal(timestamp, asset.UpdatedAt);
    }

    [Fact]
    public void MarkReady_WhileUploading_ReturnsFailureAndKeepsStatus()
    {
        VideoAsset asset = CreateAsset();

        var result = asset.MarkReady(
            StorageKey.Create("videos", "hls", "master.m3u8").Value,
            DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.UPLOADING, asset.Status);
    }

    [Fact]
    public void MarkDeleted_AfterReady_ChangesStatusToDeleted()
    {
        VideoAsset asset = CreateAsset();
        asset.MarkUploaded(DateTime.UtcNow);
        asset.MarkReady(
            StorageKey.Create("videos", "hls", "master.m3u8").Value,
            DateTime.UtcNow);

        DateTime deletedAt = new(2026, 7, 20, 11, 0, 0, DateTimeKind.Utc);
        var result = asset.MarkDeleted(deletedAt);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
        Assert.Equal(deletedAt, asset.UpdatedAt);
    }

    [Fact]
    public void MarkUploaded_AfterDeleted_ReturnsFailure()
    {
        VideoAsset asset = CreateAsset();
        asset.MarkDeleted(DateTime.UtcNow);

        var result = asset.MarkUploaded(DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.DELETED, asset.Status);
    }

    private static VideoAsset CreateAsset()
    {
        FileName fileName = FileName.Create("lesson.mp4").Value;
        ContentType contentType = ContentType.Create("video/mp4").Value;
        MediaData mediaData = MediaData.Create(fileName, contentType, 1_024, 1).Value;
        MediaOwner owner = MediaOwner.ForLesson(Guid.CreateVersion7()).Value;

        return VideoAsset.CreateForUpload(
            Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, owner).Value;
    }
}
