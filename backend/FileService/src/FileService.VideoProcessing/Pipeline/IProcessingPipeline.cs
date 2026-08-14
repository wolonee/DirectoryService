using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace FileService.VideoProcessing;

public interface IProcessingPipeline
{
    Task<UnitResult<Error>> ProcessAllStepsAsync(
        Guid videoAssetId,
        CancellationToken cancellationToken = default);
}