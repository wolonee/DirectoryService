using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing;

public interface IProcessingStepHandler
{
    StepType StepType { get; }

    Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default);
}