using System.Drawing;
using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.Assets;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 10_485_760; // 10 MB
    public const string BUCKET = "preview";
    public const string RAW_PREFIX = "raw";
    public const MediaType ALLOWED_CONTENT_TYPE = MediaType.IMAGE;
    
    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];


    protected PreviewAsset()
    {
    }

    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaUsage usage,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, mediaData, status, AssetType.PREVIEW, usage, owner, key, StorageKey.None)
    {
    }

    public static Result<PreviewAsset, Error> CreateForUpload(
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

        Result<StorageKey, Error> keyResult = StorageKey.Create(
            BUCKET,
            $"{RAW_PREFIX}/{id}",
            mediaData.FileName.Name);
        if (keyResult.IsFailure)
            return keyResult.Error;

        return new PreviewAsset(
            id,
            mediaData,
            usage,
            MediaStatus.UPLOADING,
            owner,
            keyResult.Value);
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

    public UnitResult<Error> CompleteUpload(StorageReference storageReference, DateTime timestamp)
    {
        var uploadedResult = MarkUploaded(timestamp);
        if (uploadedResult.IsFailure)
            return uploadedResult.Error;

        var readyResult = MarkReady(RawKey, storageReference, timestamp);
        if (readyResult.IsFailure)
            return readyResult.Error;
        
        return UnitResult.Success<Error>();
    }
}
