using FileService.Domain.S3Entities;
using FileService.Domain.S3Entities.MediaProcessing;

namespace FileService.Core.Abstractions;

/// <summary>
/// Фасад для пайплайна: «сообщить текущий прогресс». Внутри маппит доменный
/// VideoProcess в событие и кладёт его в очередь. Пайплайн не знает про SSE/очередь.
/// Метод не бросает и не влияет на транзакцию обработки — это побочный канал.
/// </summary>
public interface IVideoProgressReporter
{
    void Report(VideoProcess videoProcess, MediaStatus mediaStatus);
}
