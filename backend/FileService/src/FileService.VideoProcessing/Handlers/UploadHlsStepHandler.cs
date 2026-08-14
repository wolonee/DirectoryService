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
        // mock: реально в S3 ничего не грузим — собираем фиктивный reference на HLS-результат.
        // Единственный шаг, кто кладёт StorageReference в контекст. Реальный upload будет в FS-11.
        Result<StorageReference, Error> referenceResult = StorageReference.Create(
            context.VideoAsset.HlsRootKey,
            1024,
            "application/vnd.apple.mpegurl",
            eTag: null,
            checksum: null,
            lastModified: DateTime.UtcNow);
        if (referenceResult.IsFailure)
            return referenceResult.Error;

        return context with { StorageReference = referenceResult.Value };
    }
}
