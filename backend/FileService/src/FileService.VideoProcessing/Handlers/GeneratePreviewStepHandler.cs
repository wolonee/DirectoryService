using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class GeneratePreviewStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.GENERATE_PREVIEW;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // mock: превью-обложка не генерируется. Реальный ffmpeg-preview будет в FS-11.
        return context;
    }
}
