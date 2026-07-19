using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.Domain;

public class PreviewAsset : MediaAsset
{
    public const long MAX_SIZE = 5_368_709;

    public const string LOCATION = "preview";
    public const string RAW_PREFIX = "raw";
    public const string ALLOWED_CONTENT_TYPE = "image";

    public static readonly string[] AllowedExtensions = ["jpg", "jpeg", "png", "webp"];

    private PreviewAsset(
        Guid id,
        MediaData mediaData,
        MediaStatus status,
        MediaOwner owner,
        StorageKey key)
        : base(id, mediaData, status, AssetType.PREVIEW, owner, key)
    {
    }

    public static UnitResult<Error> Validate(MediaData mediaData)
    {
        if (!AllowedExtensions.Contains(mediaData.FileName.Extension))
        {
            return Error.Validation(
                "video.invalid.extension",
                $"File extension must be one of: {string.Join(", ", AllowedExtensions)}");
        }

        if (mediaData.ContentType.Category != MediaType.IMAGE)
        {
            return Error.Validation(
                "video.invalid.content-type",
                $"File content type must be {ALLOWED_CONTENT_TYPE}");
        }

        if (mediaData.Size > MAX_SIZE)
        {
            return Error.Validation(
                "video.invalid.size",
                $"File size must be less than {MAX_SIZE} bytes");
        }

        return UnitResult.Success<Error>();
    }

    public static Result<PreviewAsset, Error> CreateForUpload(Guid id, MediaData mediaData, MediaOwner owner)
    {
        var validationResult = Validate(mediaData);
        if (!validationResult.IsSuccess)
            return validationResult.Error;

        Result<StorageKey, Error> keyResult = StorageKey.Create(LOCATION, null, id.ToString());
        if (keyResult.IsFailure)
            return keyResult.Error;
        
        return new PreviewAsset(id, mediaData, MediaStatus.UPLOADING, owner, keyResult.Value);
    }
}
