using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Handlers;

public sealed class GenerateHlsStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_HLS;
    
    private readonly ILogger<GenerateHlsStepHandler> _logger;
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly IS3Provider _fileStorageProvider;
    
    public GenerateHlsStepHandler(
        ILogger<GenerateHlsStepHandler> logger,
        IFfmpegProcessRunner ffmpegProcessRunner,
        IS3Provider fileStorageProvider)
    {
        _logger = logger;
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _fileStorageProvider = fileStorageProvider;
    }

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating HLS for VideoAssetId: {VideoAssetId}", context.VideoAsset.Id);

        string inputFileUrl;

        if (!string.IsNullOrEmpty(context.MediaAssetUrl))
        {
            inputFileUrl = context.MediaAssetUrl;
        }
        else
        {
            _logger.LogDebug("InputFileUrl not cached, generating new presigned URL");

            Result<string, Error> urlResult = await _fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey);
            if (urlResult.IsFailure)
                return urlResult.Error;

            inputFileUrl = urlResult.Value;
        }

        if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
        {
            return FileErrors.HlsProcessingFailed();
        }

        if (context.VideoAsset.Metadata is null)
        {
            _logger.LogWarning("Metadata is null, progress tracking will be disabled");
        }

        var result = await _ffmpegProcessRunner
            .GenerateHlsAsync(inputFileUrl, context.HlsOutputDirectory, cancellationToken);
        
        if (result.IsFailure)
            return result.Error;

        return context;
    }
}
