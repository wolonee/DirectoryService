using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
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
    private readonly IVideoAssetRepository _videoAssetRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IProcessingJobFactory _factory;
    private readonly IVideoProgressReporter _videoProgressReporter;
    private readonly VideoProcessingOptions _options;

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IVideoProcessingService videoProcessingService,
        IVideoProcessingRepository videoProcessingRepository,
        IVideoAssetRepository videoAssetRepository,
        ITransactionManager transactionManager,
        IOptions<VideoProcessingOptions> options,
        IProcessingJobFactory factory,
        IVideoProgressReporter videoProgressReporter)
    {
        _logger = logger;
        _videoProcessingService = videoProcessingService;
        _videoProcessingRepository = videoProcessingRepository;
        _videoAssetRepository = videoAssetRepository;
        _transactionManager = transactionManager;
        _factory = factory;
        _videoProgressReporter = videoProgressReporter;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        Guid videoAssetId = Guid.Parse(context.MergedJobDataMap.GetString(VideoAssetIdKey.Name)!);

        _logger.LogInformation("Starting video processing job for VideoAssetId: {VideoAssetId}", videoAssetId);

        UnitResult<Error> result = await _videoProcessingService.ProcessVideoAsync(videoAssetId, context.CancellationToken);
        if (result.IsSuccess)
            return;

        // Транзиентный сбой оставил ПРОЦЕСС в FAILED (эта попытка провалилась),
        // а asset — в PROCESSING (pipeline его не валит). Решаем: повтор или терминал.
        Result<VideoProcess, Error> processResult = await _videoProcessingRepository
            .GetBy(vp => vp.VideoAssetId == videoAssetId, context.CancellationToken);
        if (processResult.IsFailure)
            throw new JobExecutionException(refireImmediately: false);

        VideoProcess process = processResult.Value;

        // Пробуем запланировать повтор. Успех = транзиентная ошибка и бюджет есть.
        // Неуспех = критическая ошибка или попытки исчерпаны → терминал.
        DateTime nextRetryAt = DateTime.UtcNow.AddSeconds(_options.RetryDelaySeconds);
        UnitResult<Error> scheduleRetry = process.ScheduleRetry(nextRetryAt);   // RetryCount++, NextRetryAt

        if (scheduleRetry.IsSuccess)
        {
            // Процесс остаётся FAILED до следующего захода — там LoadContext сделает Reset
            // (FAILED → IN_PROGRESS, шаги → PENDING). Asset всё это время остаётся PROCESSING.
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

        // Терминал: только теперь помечаем FAILED (asset и процесс).
        await MarkPermanentlyFailedAsync(videoAssetId, process, result.Error, context.CancellationToken);

        _logger.LogError(
            "Video processing permanently failed for VideoAssetId: {VideoAssetId} after {RetryCount} retries. Error: {Error}",
            videoAssetId,
            process.RetryCount,
            result.Error);

        throw new JobExecutionException(refireImmediately: false);
    }

    private async Task MarkPermanentlyFailedAsync(
        Guid videoAssetId,
        VideoProcess process,
        Error error,
        CancellationToken cancellationToken)
    {
        // Исчерпаны попытки: процесс ещё IN_PROGRESS → переводим в FAILED.
        // Критическая ошибка: pipeline уже поставил FAILED, тут Fail просто вернёт no-op.
        if (process.Status == ProcessingStatus.IN_PROGRESS)
            process.Fail(error.Message);

        Result<VideoAsset, Error> assetResult = await _videoAssetRepository.GetByIdAsync(videoAssetId, cancellationToken);
        if (assetResult.IsSuccess)
            assetResult.Value.MarkFailed(DateTime.UtcNow);

        await _transactionManager.SaveChangesAsync(cancellationToken);
        
        _videoProgressReporter.Report(process, MediaStatus.FAILED);
    }
}
