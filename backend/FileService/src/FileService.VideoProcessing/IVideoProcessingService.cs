using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.VideoProcessing;

public interface IVideoProcessingService
{
    Task<UnitResult<Error>> ProcessVideoAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default);
}