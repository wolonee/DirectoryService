using CSharpFunctionalExtensions;
using DirectoryService.Shared.EntitiesErrors;
using DirectoryService.Shared.Errors;
using FileService.Domain;
using FileService.Domain.Assets;

namespace FileService.Core;

public class MediaAssetFactory : IMediaAssetFactory
{
    public Result<MediaAsset, Error> CreateForUpload(
        Guid id,
        AssetType assetType,
        MediaData mediaData,
        MediaUsage usage,
        MediaOwner owner)
    {
        Result<MediaAsset, Error> CreatePreview() =>
            PreviewAsset.CreateForUpload(id, mediaData, usage, owner)
                .Map(asset => (MediaAsset)asset);

        Result<MediaAsset, Error> CreateVideo() =>
            VideoAsset.CreateForUpload(id, mediaData, usage, owner)
                .Map(asset => (MediaAsset)asset);

        return assetType switch
        {
            AssetType.PREVIEW => CreatePreview(),
            AssetType.VIDEO => CreateVideo(),
            _ => GeneralErrors.ValueIsInvalid(nameof(assetType)),
        };
    }
}
