using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class ExtractMetadataStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.EXTRACT_METADATA;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        // mock: реального ffprobe нет — кладём фиктивную metadata. Настоящее извлечение будет в FS-11.
        Result<VideoMetadata, Error> metadataResult = VideoMetadata.Create(TimeSpan.FromSeconds(30), 1280, 720);
        if (metadataResult.IsFailure)
            return metadataResult.Error;

        context.VideoAsset.SetMetadata(metadataResult.Value);

        return context;
    }
}
