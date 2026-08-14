using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class CleanupStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.CLEANUP;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // mock: пара к INITIALIZE — «освобождаем» рабочую директорию. Реальный Directory.Delete будет в FS-11.
        return context with
        {
            WorkingDirectory = null,
            HlsOutputDirectory = null,
        };
    }
}
