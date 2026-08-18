using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly IProcessingPipeline _processingPipeline;
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(
        IProcessingPipeline processingPipeline,
        ILogger<VideoProcessingService> logger)
    {
        _processingPipeline = processingPipeline;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting video processing for VideoAssetId: {VideoAssetId}",
            videoAssetId);

        UnitResult<Error> pipelineResult = await _processingPipeline
            .ProcessAllStepsAsync(videoAssetId, cancellationToken);

        return pipelineResult;
    }
}