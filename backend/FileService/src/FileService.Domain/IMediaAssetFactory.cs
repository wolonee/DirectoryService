using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.Assets;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<MediaAsset, Error> CreateForUpload(
        Guid id,
        AssetType assetType,
        MediaData mediaData,
        MediaUsage usage,
        MediaOwner owner);
}
