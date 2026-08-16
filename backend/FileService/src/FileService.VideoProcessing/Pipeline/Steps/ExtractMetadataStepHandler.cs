using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Handlers;

public sealed class ExtractMetadataStepHandler : IProcessingStepHandler
{
    private readonly ILogger<ExtractMetadataStepHandler> _logger;
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly IS3Provider _fileStorageProvider;
    
    public ExtractMetadataStepHandler(
        ILogger<ExtractMetadataStepHandler> logger,
        IFfmpegProcessRunner ffmpegProcessRunner,
        IS3Provider fileStorageProvider)
    {
        _logger = logger;
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _fileStorageProvider = fileStorageProvider;
    }
    
    public StepType StepType => StepType.EXTRACT_METADATA;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Extracting metadata for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        Result<string, Error> inputFileUrl = await _fileStorageProvider.GenerateDownloadUrlAsync(context.VideoAsset.UploadKey);
        if (inputFileUrl.IsFailure)
            return inputFileUrl.Error;
        
        context.SetMediaAssetUrl(inputFileUrl.Value);

        Result<VideoMetadata, Error> metadataResult = await _ffmpegProcessRunner.ExtractMetadataAsync(
            inputFileUrl.Value, 
            cancellationToken);

        if (metadataResult.IsFailure)
            return metadataResult.Error;

        context.VideoAsset.SetMetadata(metadataResult.Value);

        return context;
    }
}
