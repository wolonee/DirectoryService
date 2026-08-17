using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.MediaProcessing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public class VideoProcessingJob : IJob
{
    public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");

    private readonly ILogger<VideoProcessingJob> _logger;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IVideoProcessingRepository _videoProcessingRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ProcessingJobFactory _factory;
    private readonly VideoProcessingOptions _options;

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IVideoProcessingService videoProcessingService,
        IVideoProcessingRepository videoProcessingRepository,
        ITransactionManager transactionManager,
        IOptions<VideoProcessingOptions> options,
        ProcessingJobFactory factory)
    {
        _logger = logger;
        _videoProcessingService = videoProcessingService;
        _videoProcessingRepository = videoProcessingRepository;
        _transactionManager = transactionManager;
        _factory = factory;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Guid videoAssetId = context.MergedJobDataMap.GetGuid(VideoAssetIdKey.Name);

        _logger.LogInformation("Starting video processing job for VideoAssetId: {VideoAssetId}", videoAssetId);

        UnitResult<Error> result = await _videoProcessingService.ProcessVideoAsync(videoAssetId, context.CancellationToken);
        if (result.IsSuccess)
            return;

        // Pipeline уже перевёл asset и VideoProcess в FAILED. Решаем: повтор или окончательный провал.
        Result<VideoProcess, Error> processResult =
            await _videoProcessingRepository.GetBy(vp => vp.VideoAssetId == videoAssetId, context.CancellationToken);
        if (processResult.IsFailure)
            throw new JobExecutionException(refireImmediately: false);

        VideoProcess process = processResult.Value;

        // CanRetry(): RetryCount < MaxRetries && !IsCriticalError — счётчик живёт в домене, не в JobDataMap.
        if (process.CanRetry())
        {
            DateTime nextRetryAt = DateTime.UtcNow.AddSeconds(_options.RetryDelaySeconds);

            UnitResult<Error> scheduleRetry = process.ScheduleRetry(nextRetryAt);   // RetryCount++, NextRetryAt
            if (scheduleRetry.IsFailure)
                throw new JobExecutionException(refireImmediately: false);

            Result<int, Error> saveResult = await _transactionManager.SaveChangesAsync(context.CancellationToken);
            if (saveResult.IsFailure)
                throw new JobExecutionException(refireImmediately: false);

            await context.Scheduler.ScheduleJob(
                _factory.CreateRetryTrigger(videoAssetId, process.RetryCount, nextRetryAt),
                context.CancellationToken);

            _logger.LogWarning(
                "Video processing failed for VideoAssetId: {VideoAssetId}. Retry {RetryCount}/{MaxRetries} scheduled at {NextRetryAt}",
                videoAssetId,
                process.RetryCount,
                process.MaxRetries,
                nextRetryAt);

            return;
        }

        // Попытки исчерпаны или критическая ошибка — asset остаётся FAILED (терминал).
        _logger.LogError(
            "Video processing permanently failed for VideoAssetId: {VideoAssetId} after {RetryCount} retries. Error: {Error}",
            videoAssetId,
            process.RetryCount,
            result.Error);

        throw new JobExecutionException(refireImmediately: false);
    }
}
