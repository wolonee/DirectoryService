using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class GenerateHlsStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // mock: реального ffmpeg-транскодинга нет. HLS-сегменты будут генерироваться в FS-11.
        return context;
    }
}
