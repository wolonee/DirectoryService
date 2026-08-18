using FileService.Contracts.Features.Progress;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.Assets;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.Core.Features;

public static class ProgressEventMapper
{
    public static ProgressEventDto ToDto(VideoProcess videoProcess, MediaStatus mediaStatus)
    {
        // Статус для клиента берём от ASSET, а не от VideoProcess:
        // между ретраями процесс может быть FAILED, но asset остаётся PROCESSING —
        // значит для пользователя это всё ещё «в обработке», а не «ошибка».
        ProcessStatus status = mediaStatus switch
        {
            MediaStatus.UPLOADING => ProcessStatus.QUEUED,
            MediaStatus.UPLOADED => ProcessStatus.QUEUED,
            MediaStatus.PROCESSING => ProcessStatus.PROCESSING,
            MediaStatus.READY => ProcessStatus.READY,
            MediaStatus.FAILED => ProcessStatus.FAILED,
            MediaStatus.DELETED => ProcessStatus.FAILED,
            _ => throw new ArgumentOutOfRangeException(nameof(mediaStatus), mediaStatus, null),
        };

        // Активный шаг есть только пока процесс идёт. Между шагами и в терминале CurrentStep == null.
        ProcessingStep? currentStep = videoProcess.CurrentStep;

        return new ProgressEventDto
        {
            MediaAssetId = videoProcess.VideoAssetId,
            ProcessStatus = status,
            Percent = videoProcess.ProgressPercentage,
            StepOrder = currentStep?.Order,
            StepName = currentStep is null ? null : ToStepName(currentStep.StepType),
            TotalSteps = videoProcess.CountSteps(),

            // Текст ошибки показываем только в терминальном FAILED — чтобы транзиентные
            // сбои между ретраями не «мигали» ошибкой на SSE-стриме.
            Error = status == ProcessStatus.FAILED ? videoProcess.ErrorMessage : null,
            ErrorCode = null,
            PublishedAtUtc = DateTime.UtcNow,
        };
    }

    private static string ToStepName(StepType stepType) => stepType switch
    {
        StepType.INITIALIZE => "Initialize",
        StepType.EXTRACT_METADATA => "Extract Metadata",
        StepType.GENERATE_HLS => "Generate HLS",
        StepType.UPLOAD_HLS => "Upload HLS",
        StepType.GENERATE_PREVIEW => "Generate Preview",
        StepType.CLEANUP => "Clean up",
        _ => stepType.ToString(),
    };
}
