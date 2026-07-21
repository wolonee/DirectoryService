using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.Assets;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;
    public const string BUCKET = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_PREFIX = "hls";
    public const MediaType ALLOWED_CONTENT_TYPE = MediaType.IMAGE;
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];
    
    public StorageKey HlsRootKey { get; protected set; }

    protected VideoAsset()
    {
    }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaUsage usage,
        MediaStatus status,
        MediaOwner owner,
        StorageKey rawKey,
        StorageKey hlsRootKey)
        : base(id, mediaData, status, AssetType.VIDEO, usage, owner, rawKey, StorageKey.None)
    {
        HlsRootKey = hlsRootKey;
    }

    public static UnitResult<Error> ValidateForUpload(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
            return GeneralErrors.ValueIsInvalid("Extension");
        
        if (mediaData.ContentType.Category != ALLOWED_CONTENT_TYPE)
            return GeneralErrors.ValueIsInvalid("ContentType");
        
        if (mediaData.Size > MAX_SIZE)
            return GeneralErrors.ValueIsInvalid("Size");
        
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> CompleteProcessing(DateTime timestamp)
    {
        var hlsRootKeyResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);
        if (hlsRootKeyResult.IsFailure)
        {
            return hlsRootKeyResult.Error;
        }

        var readyStatusResult = MarkReady(FinalKey, timestamp);
        if (readyStatusResult.IsFailure)
        {
            return readyStatusResult.Error;
        }
        
        return UnitResult.Success<Error>();
    }
}
