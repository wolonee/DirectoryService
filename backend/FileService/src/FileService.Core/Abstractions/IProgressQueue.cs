using System.Threading.Channels;
using FileService.Contracts.Features.Progress;

namespace FileService.Core.Abstractions;

/// <summary>
/// Транспортный буфер realtime-событий прогресса между пайплайном (producer)
/// и рассылкой подписчикам (consumer). Источник истины — БД (VideoProcess),
/// очередь лишь ускоряет доставку уведомлений и переживать рестарт не обязана.
/// </summary>
public interface IProgressQueue
{
    /// <summary>
    /// Кладёт событие в очередь. НЕ блокирует: при переполнении выбрасывается
    /// самое старое событие (drop-oldest). Возвращает false только если очередь
    /// уже закрыта (shutdown приложения).
    /// </summary>
    bool TryWrite(ProgressEventDto progressEvent);

    /// <summary>Сторона чтения для consumer-а (BackgroundService).</summary>
    ChannelReader<ProgressEventDto> Reader { get; }
}
