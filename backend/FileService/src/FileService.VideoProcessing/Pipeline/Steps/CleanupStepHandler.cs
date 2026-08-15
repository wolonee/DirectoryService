using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Handlers;

public sealed class CleanupStepHandler : IProcessingStepHandler
{
    private readonly ILogger<CleanupStepHandler> _logger;
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly IS3Provider _fileStorageProvider;
    
    public CleanupStepHandler(
        ILogger<CleanupStepHandler> logger,
        IFfmpegProcessRunner ffmpegProcessRunner,
        IS3Provider fileStorageProvider)
    {
        _logger = logger;
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _fileStorageProvider = fileStorageProvider;
    }
    
    public StepType StepType => StepType.CLEANUP;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Cleaning up temporary files for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.WorkingDirectory))
        {
            _logger.LogWarning("Working directory is not set, skipping cleanup");
            return await Task.FromResult(context);
        }

        UnitResult<Error> deleteResult = await _fileStorageProvider
            .DeleteObjectAsync(context.VideoAsset.UploadKey, cancellationToken);

        if (deleteResult.IsFailure)
        {
            _logger.LogWarning(
                "Failed to delete raw file from storage for VideoAssetId: {VideoAssetId}. Error: {Error}",
                context.VideoAsset.Id,
                deleteResult.Error);
        }
        else
        {
            _logger.LogDebug(
                "Raw file deleted from storage for VideoAssetId: {VideoAssetId}",
                context.VideoAsset.Id);
        }

        try
        {
            if (Directory.Exists(context.WorkingDirectory))
            {
                Directory.Delete(
                    context.WorkingDirectory,
                    recursive: true);

                _logger.LogDebug(
                    "Working directory deleted: {WorkingDirectory}",
                    context.WorkingDirectory);

                context.Cleanup();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to delete working directory: {WorkingDirectory}. Will be cleaned up later.",
                context.WorkingDirectory);
        }

        return await Task.FromResult(context);
    }
}
