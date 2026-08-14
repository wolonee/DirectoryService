using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing;

public sealed record ProcessingContext
{
    public required VideoProcess VideoProcess { get; init; }

    public required VideoAsset VideoAsset { get; init; }

    public string? WorkingDirectory { get; init; }

    public string? HlsOutputDirectory { get; init; }
    
    public string? MediaAssetUrl { get; init; }
    
    public StorageReference? StorageReference { get; init; }
}