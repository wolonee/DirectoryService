using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.IntegrationTests.Infrastructure;
using FileService.VideoProcessing;
using Microsoft.Extensions.DependencyInjection;

namespace FileService.IntegrationTests;

// Реальный e2e: настоящий файл → ffprobe/ffmpeg → HLS в MinIO → READY.
// Требует Docker (Testcontainers) и установленного ffmpeg на хосте (локально + в CI).
public class VideoProcessingIntegrationTests : FileServiceIntegrationTestBase
{
    public VideoProcessingIntegrationTests(FileServiceTestWebFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ProcessVideo_WithRealFfmpeg_GeneratesHlsAndMarksReady()
    {
        // Arrange: video-asset в UPLOADED + реальный sample.mp4 в MinIO по raw-ключу.
        VideoAsset asset = await SeedUploadedVideoAsync();

        // Act: полный pipeline (ffprobe → ffmpeg → HLS → upload → cleanup).
        UnitResult<Error> result = await RunPipelineAsync(asset.Id);

        // Assert: успех + статус/metadata в БД.
        Assert.True(result.IsSuccess, result.IsFailure ? result.Error.Message : string.Empty);

        var reloaded = (VideoAsset)await GetAssetAsync(asset.Id);
        Assert.Equal(MediaStatus.READY, reloaded.Status);
        Assert.NotNull(reloaded.Metadata);
        Assert.True(reloaded.Metadata!.Duration > TimeSpan.Zero);

        // HLS реально лежит в MinIO под videos/hls/{id}/.
        List<string> hlsKeys = await ListKeysAsync("videos", $"hls/{asset.Id}");
        Assert.Contains(hlsKeys, key => key.EndsWith("master.m3u8"));
        Assert.Contains(hlsKeys, key => key.EndsWith(".ts"));
    }

    private async Task<VideoAsset> SeedUploadedVideoAsync()
    {
        byte[] videoBytes = ReadSample();

        MediaData mediaData = MediaData.Create(
            FileName.Create("sample.mp4").Value,
            ContentType.Create("video/mp4").Value,
            videoBytes.Length).Value;
        MediaOwner owner = MediaOwner.ForLesson(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;
        VideoAsset asset = VideoAsset.CreateForUpload(Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, owner).Value;
        asset.MarkUploaded(DateTime.UtcNow);

        await Factory.ExecuteInDbAsync(async db =>
        {
            db.Add(asset);
            await db.SaveChangesAsync();
            return true;
        });

        // Кладём исходник в MinIO по тому ключу, откуда ffmpeg его прочитает (UploadKey = RawKey у видео).
        StorageKey rawKey = asset.UploadKey;
        using var stream = new MemoryStream(videoBytes);
        await Factory.S3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = rawKey.Bucket,
            Key = rawKey.Value,
            InputStream = stream,
            ContentType = "video/mp4",
        });

        return asset;
    }

    private async Task<UnitResult<Error>> RunPipelineAsync(Guid videoAssetId)
    {
        await using AsyncServiceScope scope = Factory.Services.CreateAsyncScope();
        var pipeline = scope.ServiceProvider.GetRequiredService<IProcessingPipeline>();
        return await pipeline.ProcessAllStepsAsync(videoAssetId);
    }

    private async Task<List<string>> ListKeysAsync(string bucket, string prefix)
    {
        ListObjectsV2Response response = await Factory.S3Client.ListObjectsV2Async(new ListObjectsV2Request
        {
            BucketName = bucket,
            Prefix = prefix,
        });

        return response.S3Objects?.Select(o => o.Key).ToList() ?? [];
    }

    private static byte[] ReadSample() =>
        File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Assets", "sample.mp4"));
}
