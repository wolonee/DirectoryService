using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;

namespace FileService.Domain.S3Entities.Assets;

public enum AssetType
{
    VIDEO,
    PREVIEW,
    AVATAR,
}

public static class AssetTypeExtensions
{
    public static Result<AssetType, Error> ToAssetType(this string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "video" => AssetType.VIDEO,
            "preview" => AssetType.PREVIEW,
            _ => GeneralErrors.ValueIsInvalid(nameof(AssetType)),
        };
    }
}
