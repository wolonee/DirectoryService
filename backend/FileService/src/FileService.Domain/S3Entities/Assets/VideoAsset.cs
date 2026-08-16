using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.S3Entities.Assets;

public class VideoAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709_120;
    public const string BUCKET = "videos";
    public const string RAW_PREFIX = "raw";
    public const string HLS_PREFIX = "hls";
    public const MediaType ALLOWED_CONTENT_TYPE = MediaType.VIDEO;
    public const string MASTER_PLAYLIST_NAME = "master.m3u8";
    public const string STREAM_PLAYLIST_PATTERN = "%v_stream.m3u8";
    public const string SEGMENT_FILE_PATTERN = "%v_%06d.ts";
    public static readonly string[] AllowedExtensions = ["mp4", "mkv", "avi", "mov"];
    
    public StorageKey HlsRootKey { get; protected set; } = StorageKey.None;
    
    public VideoMetadata? Metadata { get; private set; }

    protected VideoAsset()
    {
    }

    private VideoAsset(
        Guid id,
        MediaData mediaData,
        MediaUsage usage,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key,
        StorageKey hlsRootKey)
        : base(id, mediaData, status, AssetType.VIDEO, usage, owner, key, false)
    {
        HlsRootKey = hlsRootKey;
    }

    public static Result<VideoAsset, Error> CreateForUpload(
        Guid id,
        MediaData mediaData,
        MediaUsage usage,
        MediaOwner owner)
    {
        if (id == Guid.Empty)
            return GeneralErrors.ValueIsInvalid(nameof(id));

        UnitResult<Error> validationResult = ValidateForUpload(mediaData);
        if (validationResult.IsFailure)
            return validationResult.Error;

        Result<StorageKey, Error> rawKeyResult = StorageKey.Create(
            BUCKET,
            $"{RAW_PREFIX}/{id}",
            mediaData.FileName.Name);
        if (rawKeyResult.IsFailure)
            return rawKeyResult.Error;

        Result<StorageKey, Error> hlsRootKeyResult = StorageKey.Create(
            BUCKET,
            HLS_PREFIX,
            id.ToString());
        if (hlsRootKeyResult.IsFailure)
            return hlsRootKeyResult.Error;

        return new VideoAsset(
            id,
            mediaData,
            usage,
            MediaStatus.UPLOADING,
            owner,
            rawKeyResult.Value,
            hlsRootKeyResult.Value);
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

    public UnitResult<Error> CompleteProcessing(StorageReference? storageReference, DateTime timestamp)
    {
        if (storageReference is null)
            return Error.Validation("video.storage-reference.required", "Cannot complete processing without a storage reference.");
        
        Result<StorageKey, Error> hlsRootKeyResult = HlsRootKey.AppendSegment(MASTER_PLAYLIST_NAME);
        if (hlsRootKeyResult.IsFailure)
        {
            return hlsRootKeyResult.Error;
        }

        UnitResult<Error> readyStatusResult = MarkReady(hlsRootKeyResult.Value, storageReference, timestamp);
        if (readyStatusResult.IsFailure)
        {
            return readyStatusResult.Error;
        }
        
        return UnitResult.Success<Error>();
    }

    public void SetMetadata(VideoMetadata metadata)
    {
        Metadata = metadata;
    }

    public override bool RequiresProcessing() => true;

    public UnitResult<Error> StartProcessing()
    {
        if (Status != MediaStatus.UPLOADED && Status != MediaStatus.FAILED)
            return Error.Validation("asset.invalid.status.transition", "Can only start processing from UPLOADED or FAILED status");

        if (!RequiresProcessing())
            return Error.Validation("asset.processing.not.required", "This asset type does not require processing");

        Status = MediaStatus.PROCESSING;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
    
    public Result<StorageKey, Error> GetHlsRootKey()
    {
        return StorageKey.Create(BUCKET, HLS_PREFIX, Id.ToString());
    }
    
    public Result<StorageKey, Error> GetHlsMasterPlaylistKey()
    {
        Result<StorageKey, Error> hlsRoot = GetHlsRootKey();
        if (hlsRoot.IsFailure)
            return hlsRoot.Error;

        return hlsRoot.Value.AppendKey(MASTER_PLAYLIST_NAME);
    }
    
    public UnitResult<Error> SetHlsMasterPlaylistKey(StorageKey value)
    {
        if (Status != MediaStatus.PROCESSING)
            return Error.Validation("video.invalid.status", "Can only set processed data during processing");

        FinalKey = value;
        UpdatedAt = DateTime.UtcNow;

        return UnitResult.Success<Error>();
    }
}
