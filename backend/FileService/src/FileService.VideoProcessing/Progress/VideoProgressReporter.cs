using FileService.Contracts.Features.Progress;
using FileService.Core.Abstractions;
using FileService.Core.Features;
using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;
using Microsoft.Extensions.Logging;

namespace FileService.VideoProcessing.Progress;

public class VideoProgressReporter : IVideoProgressReporter
{
    private readonly IProgressQueue _queue;
    private readonly ILogger<VideoProgressReporter> _logger;

    public VideoProgressReporter(IProgressQueue queue, ILogger<VideoProgressReporter> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    public void Report(VideoProcess videoProcess, MediaStatus mediaStatus)
    {
        // 1. Домен → контракт события (нормализация статуса, percent, шаг).
        ProgressEventDto progressEvent = ProgressEventMapper.ToDto(videoProcess, mediaStatus);

        // 2. Кладём в очередь и забываем. TryWrite не блокирует пайплайн;
        //    false = очередь закрыта (shutdown) — прогресс не критичен, просто лог.
        bool written = _queue.TryWrite(progressEvent);
        if (!written)
        {
            _logger.LogDebug(
                "Progress event dropped (queue closed) for VideoAssetId: {VideoAssetId}",
                progressEvent.MediaAssetId);
        }
    }
}
