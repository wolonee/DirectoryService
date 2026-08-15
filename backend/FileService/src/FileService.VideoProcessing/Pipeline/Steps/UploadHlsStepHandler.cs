using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Core.Abstractions;
using FileService.Core.Options.FileStorageOptions;
using FileService.Domain;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;
using FileService.VideoProcessing.FfmpegProcess;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileService.VideoProcessing.Handlers;

public sealed class UploadHlsStepHandler : IProcessingStepHandler
{
    private readonly ILogger<UploadHlsStepHandler> _logger;
    private readonly FileStorageOptions _options;
    private readonly IFfmpegProcessRunner _ffmpegProcessRunner;
    private readonly IS3Provider _fileStorageProvider;
    
    public UploadHlsStepHandler(
        ILogger<UploadHlsStepHandler> logger,
        IOptions<FileStorageOptions> options,
        IFfmpegProcessRunner ffmpegProcessRunner,
        IS3Provider fileStorageProvider)
    {
        _logger = logger;
        _options = options.Value;
        _ffmpegProcessRunner = ffmpegProcessRunner;
        _fileStorageProvider = fileStorageProvider;
    }
    
    public StepType StepType => StepType.UPLOAD_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Uploading HLS to S3 for VideoAssetId: {VideoAssetId}",
            context.VideoAsset.Id);

        if (string.IsNullOrWhiteSpace(context.HlsOutputDirectory))
            return FileErrors.HlsProcessingFailed("HLS output directory is not set");

        if (!Directory.Exists(context.HlsOutputDirectory))
            return FileErrors.HlsProcessingFailed("HLS output directory does not exist");

        string[] hlsFiles = Directory.GetFiles(
            context.HlsOutputDirectory,
            "*.*",
            SearchOption.AllDirectories);

        if (hlsFiles.Length == 0)
            return FileErrors.HlsProcessingFailed("No HLS files found in output directory");

        Result<StorageKey, Error> hlsRootKey = context.VideoAsset.GetHlsRootKey();
        if (hlsRootKey.IsFailure)
            return hlsRootKey.Error;

        using var throttler = new SemaphoreSlim(_options.UploadDegreeOfParallelism);

        Task<UnitResult<Error>>[] uploadTasks = hlsFiles.Select(async file =>
        {
            await throttler.WaitAsync(cancellationToken);

            try
            {
                return await UploadHlsFileAsync(
                    hlsRootKey.Value,
                    file,
                    cancellationToken);
            }
            finally
            {
                throttler.Release();
            }
        }).ToArray();

        UnitResult<Error>[] results = await Task.WhenAll(uploadTasks);

        UnitResult<Error> firstFailure = results.FirstOrDefault(r => r.IsFailure);
        if (firstFailure.IsFailure)
            return firstFailure.Error;

        _logger.LogInformation(
            "Successfully uploaded {FileCount} HLS files for VideoAssetId: {VideoAssetId}",
            hlsFiles.Length,
            context.VideoAsset.Id);

        Result<StorageKey, Error> masterPlaylistKey = context.VideoAsset.GetHlsMasterPlaylistKey();
        if (masterPlaylistKey.IsFailure)
            return masterPlaylistKey.Error;

        UnitResult<Error> setKeyResult = context.VideoAsset.SetHlsMasterPlaylistKey(masterPlaylistKey.Value);
        if (setKeyResult.IsFailure)
            return setKeyResult.Error;
        
        long size = new FileInfo(masterPlaylistKey.Value.Value).Length;
        
        Result<StorageReference, Error> storageReferenceResult = StorageReference
            .Create(masterPlaylistKey.Value, size, "video", null, null, DateTime.UtcNow);
        
        if (storageReferenceResult.IsFailure)
            return storageReferenceResult.Error;
        
        context.SetStorageReference(storageReferenceResult.Value);
        
        return context;
    }
    
    private async Task<UnitResult<Error>> UploadHlsFileAsync(
        StorageKey hlsRootKey,
        string localFilePath,
        CancellationToken cancellationToken)
    {
        string fileName = Path.GetFileName(localFilePath);

        Result<StorageKey, Error> storageKey = hlsRootKey.AppendKey(fileName);
        if (storageKey.IsFailure)
            return storageKey.Error;

        string contentType = GetContentType(localFilePath);

        await using FileStream fileStream = File.OpenRead(localFilePath);

        return await _fileStorageProvider.UploadFileAsync(
            storageKey.Value,
            fileStream,
            contentType,
            cancellationToken);
    }

    private string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".m3u8" => "application/vnd.apple.mpegurl",
            ".ts" => "video/mp2t",
            _ => "application/octet-stream"
        };
    }
}
