using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing;

public sealed record ProcessingContext
{
    private const string HLS_SUBDIRECTORY = "hls";
    public required VideoProcess VideoProcess { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; private set; }

    public string? HlsOutputDirectory { get; private set; }

    public string? MediaAssetUrl { get; private set; }

    public StorageReference? StorageReference { get; private set; }


    public UnitResult<Error> CreateWorkingDirectory()
    {
        try
        {
            WorkingDirectory = Directory.CreateTempSubdirectory("video-processing").FullName;

            HlsOutputDirectory = Path.Combine(WorkingDirectory, HLS_SUBDIRECTORY);
            Directory.CreateDirectory(HlsOutputDirectory);
        }
        catch (Exception ex)
        {
            return Error.Failure(
                "working.directory.creation",
                $"Failed to create working directory: {ex.Message}");
        }

        return UnitResult.Success<Error>();
    }

    public void SetMediaAssetUrl(string mediaAssetUrl)
    {
        MediaAssetUrl = mediaAssetUrl;
    }
    
    public void SetStorageReference(StorageReference storageReference)
    {
        StorageReference = storageReference;
    }
    
    internal void Cleanup()
    {
        WorkingDirectory = null;
        HlsOutputDirectory = null;
        MediaAssetUrl = null;
    }
}