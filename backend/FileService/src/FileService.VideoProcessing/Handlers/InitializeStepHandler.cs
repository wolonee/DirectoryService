using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class InitializeStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.INITIALIZE;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // mock: реального I/O нет — просто заводим пути. Реальный Directory.CreateDirectory будет в FS-11.
        string workingDirectory = $"/tmp/video-processing/{context.VideoAsset.Id}";
        string hlsOutputDirectory = $"{workingDirectory}/hls";

        return context with
        {
            WorkingDirectory = workingDirectory,
            HlsOutputDirectory = hlsOutputDirectory,
        };
    }
}
