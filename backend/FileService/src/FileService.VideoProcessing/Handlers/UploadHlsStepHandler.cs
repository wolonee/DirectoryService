using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.VideoProcessing.Handlers;

public sealed class UploadHlsStepHandler : IProcessingStepHandler
{
    public StepType StepType => StepType.UPLOAD_HLS;

    public async Task<Result<ProcessingContext, Error>> ExecuteAsync(
        ProcessingContext context,
        CancellationToken cancellationToken = default)
    {
        StorageReference fakeRef = null;

        return context with { StorageReference = fakeRef };
    }
}