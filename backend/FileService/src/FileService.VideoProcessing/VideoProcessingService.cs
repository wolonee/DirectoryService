using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing;

public class VideoProcessingService
{
    private readonly ILogger<VideoProcessingService> _logger;

    public VideoProcessingService(
        ILogger<VideoProcessingService> logger)
    {
        _logger = logger;
    }

    public async Task<UnitResult<Error>> ProcessVideoAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("");
        
        // pipeline
        return new UnitResult<Error>();
    }
    
    
}