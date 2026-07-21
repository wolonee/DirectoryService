using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.Assets;

namespace FileService.Domain;

public interface IMediaAssetFactory
{
    Result<VideoAsset, Error> CreateVideoForUpload(MediaData mediaData, MediaOwner owner);

    Result<PreviewAsset, Error> CreatePreviewForUpload(MediaData mediaData, MediaOwner owner);
}
