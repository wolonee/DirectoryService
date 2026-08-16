using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace FileService.VideoProcessing.Jobs;

[DisallowConcurrentExecution]
public class VideoProcessingJob : IJob
{
    public static readonly JobKey VideoAssetIdKey = new("VideoAssetId");
    public static readonly JobKey RetryCountKey = new("RetryCountKey");

    private readonly ILogger<VideoProcessingJob> _logger;
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IVideoAssetRepository _videoAssetRepository;
    private readonly ProcessingJobFactory _factory;
    private readonly VideoProcessingOptions _options;

    public VideoProcessingJob(
        ILogger<VideoProcessingJob> logger,
        IVideoProcessingService videoProcessingService,
        IVideoAssetRepository videoAssetRepository,
        IOptions<VideoProcessingOptions> options,
        ProcessingJobFactory factory)
    {
        _logger = logger;
        _videoProcessingService = videoProcessingService;
        _videoAssetRepository = videoAssetRepository;
        _factory = factory;
        _options = options.Value;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap dataMap = context.MergedJobDataMap;
        Guid videoAssetId = dataMap.GetGuid(VideoAssetIdKey.Name);
        int retryCount = dataMap.GetInt(RetryCountKey.Name);

        _logger.LogInformation("Starting video processing job for VideoAssetId: {VideoAssetId}", videoAssetId);

        var result = await _videoProcessingService.ProcessVideoAsync(videoAssetId, context.CancellationToken);
        if (result.IsSuccess)
            return;
        
        Result<VideoAsset, Error> assetResult = await _videoAssetRepository.GetByIdAsync(videoAssetId, context.CancellationToken);
        if (assetResult.IsFailure)
            throw new Exception(assetResult.Error.Message);
        
        var asset = assetResult.Value;
        
        bool canRetry = asset.Status != MediaStatus.FAILED && retryCount < _options.MaxRetries;
        if (canRetry)
        {
            var startAt = DateTime.UtcNow.AddSeconds(_options.RetryDelaySeconds);
            await context.Scheduler.ScheduleJob(_factory.CreateRetryTrigger(videoAssetId, retryCount + 1, startAt), context.CancellationToken);
            return;
        }

        throw new JobExecutionException(refireImmediately: false);
    }
}