using System.Linq.Expressions;
using System.Data;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.MultipartUpload.CompleteMultipartUpload;
using FileService.Contracts.Features.MultipartUpload.StartMultipartUpload;
using FileService.Contracts.Features.Simple.InitiateUpload;
using FileService.Contracts.Shared;
using FileService.Core.Abstractions;
using FileService.Core.Models;
using FileService.Core.Options.FileStorageOptions;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using FileService.VideoProcessing.Handlers;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace FileService.VideoProcessing.UnitTests;

// Реальные шаги pipeline с fake ffmpeg + fake S3 — без Docker и без установленного ffmpeg (CI-safe, урок 017).
public class RealPipelineProcessingTests
{
    [Fact]
    public async Task RealPipeline_WithFakeFfmpeg_ProcessesVideoToReady()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        var fakeS3 = new FakeS3Provider();
        List<IProcessingStepHandler> handlers = BuildHandlers(new FakeFfmpegProcessRunner(), fakeS3);
        ProcessingPipeline pipeline = CreatePipeline(asset, handlers);

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.NotNull(asset.Metadata);
        Assert.Contains(fakeS3.UploadedKeys, k => k.EndsWith("master.m3u8"));
        Assert.Contains(fakeS3.UploadedKeys, k => k.EndsWith(".ts"));
    }

    [Fact]
    public async Task RealPipeline_WhenFfmpegFails_MarksVideoFailed()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        List<IProcessingStepHandler> handlers = BuildHandlers(new FakeFfmpegProcessRunner(failHls: true), new FakeS3Provider());
        ProcessingPipeline pipeline = CreatePipeline(asset, handlers);

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.FAILED, asset.Status);
    }

    // ---------- Helpers ----------

    private static List<IProcessingStepHandler> BuildHandlers(IFfmpegProcessRunner ffmpeg, IS3Provider s3) =>
    [
        new InitializeStepHandler(NullLogger<InitializeStepHandler>.Instance),
        new ExtractMetadataStepHandler(NullLogger<ExtractMetadataStepHandler>.Instance, ffmpeg, s3),
        new GenerateHlsStepHandler(NullLogger<GenerateHlsStepHandler>.Instance, ffmpeg, s3),
        new UploadHlsStepHandler(
            NullLogger<UploadHlsStepHandler>.Instance,
            Options.Create(new FileStorageOptions { UploadDegreeOfParallelism = 2 }),
            ffmpeg,
            s3),
        new GeneratePreviewStepHandler(),
        new CleanupStepHandler(NullLogger<CleanupStepHandler>.Instance, ffmpeg, s3),
    ];

    private static ProcessingPipeline CreatePipeline(VideoAsset asset, IEnumerable<IProcessingStepHandler> handlers) =>
        new(
            NullLogger<ProcessingPipeline>.Instance,
            new FakeVideoProcessingRepository(),
            new FakeVideoAssetRepository(asset),
            new FakeTransactionManager(),
            Options.Create(new VideoProcessingOptions()),
            handlers);

    private static VideoAsset CreateUploadedVideoAsset()
    {
        MediaData mediaData = MediaData.Create(
            FileName.Create("lesson.mp4").Value,
            ContentType.Create("video/mp4").Value,
            1_024).Value;
        MediaOwner owner = MediaOwner.ForLesson(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;
        VideoAsset asset = VideoAsset.CreateForUpload(Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, owner).Value;
        asset.MarkUploaded(DateTime.UtcNow);
        return asset;
    }

    private sealed class FakeFfmpegProcessRunner : IFfmpegProcessRunner
    {
        private readonly bool _failHls;

        public FakeFfmpegProcessRunner(bool failHls = false) => _failHls = failHls;

        public Task<Result<VideoMetadata, Error>> ExtractMetadataAsync(string inputFileUrl, CancellationToken ct = default) =>
            Task.FromResult(VideoMetadata.Create(TimeSpan.FromSeconds(30), 1280, 720));

        public Task<UnitResult<Error>> GenerateHlsAsync(string inputFileUrl, string outputDirectory, CancellationToken ct = default)
        {
            if (_failHls)
                return Task.FromResult<UnitResult<Error>>(Error.Failure("fake.ffmpeg.failed", "boom"));

            // имитируем результат ffmpeg: master-плейлист + один сегмент
            File.WriteAllText(Path.Combine(outputDirectory, "master.m3u8"), "#EXTM3U\n#EXT-X-VERSION:3\n");
            File.WriteAllText(Path.Combine(outputDirectory, "segment0.ts"), "fake-segment-bytes");
            return Task.FromResult(UnitResult.Success<Error>());
        }
    }

    private sealed class FakeS3Provider : IS3Provider
    {
        public List<string> UploadedKeys { get; } = [];

        public Task<UnitResult<Error>> UploadFileAsync(StorageKey storageKey, FileStream fileStream, string contentType, CancellationToken cancellationToken)
        {
            UploadedKeys.Add(storageKey.FullPath);
            return Task.FromResult(UnitResult.Success<Error>());
        }

        public Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey) =>
            Task.FromResult(Result.Success<string, Error>("http://fake-storage/video.mp4"));

        public Task<Result<DeleteObjectResult, Error>> DeleteObjectAsync(StorageKey storageKey, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<DeleteObjectResult, Error>(new DeleteObjectResult(null, null)));

        // Не используются в processing-pipeline:
        public Task<Result<string, Error>> StartMultipartUploadAsync(StorageKey storageKey, ContentType contentType, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<Result<IReadOnlyList<MultipartPartUploadDto>, Error>> GenerateAllChunksUploadUrlsAsync(StorageKey storageKey, string uploadId, int totalChunks, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<Result<string, Error>> CompleteMultipartUploadAsync(StorageKey storageKey, string uploadId, IReadOnlyList<PartETagDto> partETags, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<UnitResult<Error>> AbortMultipartUploadAsync(StorageKey storageKey, string uploadId, CancellationToken cancellationToken) => throw new NotImplementedException();
        public void Dispose() { }
        public Task<Result<PresignedUploadDto, Error>> GenerateUploadUrlAsync(StorageKey storageKey, ContentType contentType, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<Result<MediaUrl[], Error>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<Result<ObjectMetadataDto, Error>> GetObjectMetadataAsync(StorageKey storageKey, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<UnitResult<Error>> EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class FakeVideoProcessingRepository : IVideoProcessingRepository
    {
        public Task<Result<VideoProcess, Error>> GetBy(Expression<Func<VideoProcess, bool>> predicate, CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Failure<VideoProcess, Error>(GeneralErrors.NotFound(Guid.Empty, "VideoProcess")));

        public void Add(VideoProcess videoProcessing) { }
    }

    private sealed class FakeVideoAssetRepository : IVideoAssetRepository
    {
        private readonly VideoAsset _asset;

        public FakeVideoAssetRepository(VideoAsset asset) => _asset = asset;

        public Task<Result<VideoAsset, Error>> GetByIdAsync(Guid videoId, CancellationToken cancellationToken) =>
            Task.FromResult(Result.Success<VideoAsset, Error>(_asset));
    }

    private sealed class FakeTransactionManager : ITransactionManager
    {
        public Task<Result<int, Error>> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Success<int, Error>(0));

        public Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
