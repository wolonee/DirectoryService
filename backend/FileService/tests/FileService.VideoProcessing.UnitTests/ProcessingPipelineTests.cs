using System.Data;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Options;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace FileService.VideoProcessing.UnitTests;

public class ProcessingPipelineTests
{
    private static readonly StepType[] ExpectedOrder =
    [
        StepType.INITIALIZE,
        StepType.EXTRACT_METADATA,
        StepType.GENERATE_HLS,
        StepType.UPLOAD_HLS,
        StepType.GENERATE_PREVIEW,
        StepType.CLEANUP,
    ];

    [Fact]
    public async Task HappyPath_RunsAllStepsInOrder_MarksAssetReady()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        var log = new List<StepType>();
        ProcessingPipeline pipeline = CreatePipeline(asset, AllMockHandlers(log));

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsSuccess);
        Assert.Equal(MediaStatus.READY, asset.Status);
        Assert.Equal(ExpectedOrder, log);
    }

    [Fact]
    public async Task StepFailure_MarksAssetFailed_AndStopsRemainingSteps()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        var log = new List<StepType>();
        var handlers = AllMockHandlers(log);

        // GENERATE_HLS возвращает ошибку в середине конвейера.
        handlers[2] = new RecordingStepHandler(
            StepType.GENERATE_HLS,
            log,
            _ => Error.Failure("test.step.failed", "boom"));

        ProcessingPipeline pipeline = CreatePipeline(asset, handlers);

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.FAILED, asset.Status);
        Assert.Equal(
            new[] { StepType.INITIALIZE, StepType.EXTRACT_METADATA, StepType.GENERATE_HLS },
            log);
    }

    [Fact]
    public async Task StepThrows_IsCaught_MarksAssetFailed()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        var log = new List<StepType>();
        var handlers = AllMockHandlers(log);

        handlers[2] = new RecordingStepHandler(
            StepType.GENERATE_HLS,
            log,
            _ => throw new InvalidOperationException("boom"));

        ProcessingPipeline pipeline = CreatePipeline(asset, handlers);

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.FAILED, asset.Status);
        Assert.DoesNotContain(StepType.UPLOAD_HLS, log);
    }

    [Fact]
    public async Task MissingHandler_MarksAssetFailed_AndStopsAtMissingStep()
    {
        VideoAsset asset = CreateUploadedVideoAsset();
        var log = new List<StepType>();

        // Нет обработчика для GENERATE_HLS.
        var handlers = new List<IProcessingStepHandler>
        {
            new RecordingStepHandler(StepType.INITIALIZE, log),
            new RecordingStepHandler(StepType.EXTRACT_METADATA, log),
            new RecordingStepHandler(StepType.UPLOAD_HLS, log, SetFakeReference),
            new RecordingStepHandler(StepType.GENERATE_PREVIEW, log),
            new RecordingStepHandler(StepType.CLEANUP, log),
        };

        ProcessingPipeline pipeline = CreatePipeline(asset, handlers);

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.FAILED, asset.Status);
        Assert.Equal(
            new[] { StepType.INITIALIZE, StepType.EXTRACT_METADATA },
            log);
    }

    [Fact]
    public async Task InvalidStartStatus_DoesNotProcess()
    {
        // Ассет остаётся UPLOADING (не UPLOADED) — StartProcessing должен упасть.
        VideoAsset asset = CreateVideoAsset();
        var log = new List<StepType>();
        ProcessingPipeline pipeline = CreatePipeline(asset, AllMockHandlers(log));

        UnitResult<Error> result = await pipeline.ProcessAllStepsAsync(asset.Id);

        Assert.True(result.IsFailure);
        Assert.Equal(MediaStatus.UPLOADING, asset.Status);
        Assert.Empty(log);
    }

    // ---------- Helpers ----------

    private static List<IProcessingStepHandler> AllMockHandlers(List<StepType> log) =>
    [
        new RecordingStepHandler(StepType.INITIALIZE, log),
        new RecordingStepHandler(StepType.EXTRACT_METADATA, log),
        new RecordingStepHandler(StepType.GENERATE_HLS, log),
        new RecordingStepHandler(StepType.UPLOAD_HLS, log, SetFakeReference),
        new RecordingStepHandler(StepType.GENERATE_PREVIEW, log),
        new RecordingStepHandler(StepType.CLEANUP, log),
    ];

    private static Result<ProcessingContext, Error> SetFakeReference(ProcessingContext context)
    {
        StorageReference reference = StorageReference.Create(
            context.VideoAsset.HlsRootKey,
            1024,
            "application/vnd.apple.mpegurl",
            null,
            null,
            DateTime.UtcNow).Value;

        context.SetStorageReference(reference);
        return context;
    }

    private static ProcessingPipeline CreatePipeline(VideoAsset asset, IEnumerable<IProcessingStepHandler> handlers) =>
        new(
            NullLogger<ProcessingPipeline>.Instance,
            new FakeVideoProcessingRepository(),
            new FakeVideoAssetRepository(asset),
            new FakeTransactionManager(),
            Options.Create(new VideoProcessingOptions()),
            handlers);

    private static VideoAsset CreateVideoAsset()
    {
        MediaData mediaData = MediaData.Create(
            FileName.Create("lesson.mp4").Value,
            ContentType.Create("video/mp4").Value,
            1_024).Value;
        MediaOwner owner = MediaOwner.ForLesson(Guid.CreateVersion7(), Guid.CreateVersion7()).Value;

        return VideoAsset.CreateForUpload(Guid.CreateVersion7(), mediaData, MediaUsage.LESSON_VIDEO, owner).Value;
    }

    private static VideoAsset CreateUploadedVideoAsset()
    {
        VideoAsset asset = CreateVideoAsset();
        asset.MarkUploaded(DateTime.UtcNow);
        return asset;
    }

    private sealed class RecordingStepHandler : IProcessingStepHandler
    {
        private readonly List<StepType> _log;
        private readonly Func<ProcessingContext, Result<ProcessingContext, Error>>? _behavior;

        public RecordingStepHandler(
            StepType stepType,
            List<StepType> log,
            Func<ProcessingContext, Result<ProcessingContext, Error>>? behavior = null)
        {
            StepType = stepType;
            _log = log;
            _behavior = behavior;
        }

        public StepType StepType { get; }

        public Task<Result<ProcessingContext, Error>> ExecuteAsync(
            ProcessingContext context,
            CancellationToken cancellationToken = default)
        {
            _log.Add(StepType);
            Result<ProcessingContext, Error> result = _behavior is not null ? _behavior(context) : context;
            return Task.FromResult(result);
        }
    }

    private sealed class FakeVideoProcessingRepository : IVideoProcessingRepository
    {
        public Task<Result<VideoProcess, Error>> GetBy(
            Expression<Func<VideoProcess, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(Result.Failure<VideoProcess, Error>(GeneralErrors.NotFound(Guid.Empty, "VideoProcess")));

        public void Add(VideoProcess videoProcessing)
        {
        }
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
