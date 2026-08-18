using System.Threading.Channels;
using FileService.Contracts.Features.Progress;
using FileService.Core.Abstractions;

namespace FileService.Core.Features.Progress;

/// <summary>
/// In-memory очередь прогресса поверх System.Threading.Channels.
/// Регистрируется как singleton: один экземпляр канала на всё приложение —
/// пайплайн пишет, единственный consumer читает.
/// </summary>
public sealed class InMemoryProgressQueue : IProgressQueue
{
    // Ёмкость ленты. Хватает с запасом: шаги видео идут не чаще пары раз в секунду,
    // а consumer разгребает почти мгновенно. Переполнение возможно лишь при всплеске.
    private const int Capacity = 1000;

    private readonly Channel<ProgressEventDto> _channel;

    public InMemoryProgressQueue()
    {
        var options = new BoundedChannelOptions(Capacity)
        {
            // Лента полна → выкидываем самое старое событие, а не блокируем пайплайн.
            // Старый промежуточный кадр прогресса не важен, если едет более свежий.
            FullMode = BoundedChannelFullMode.DropOldest,

            // Читатель ровно один (наш ProgressConsumer) — рантайм оптимизирует под это.
            SingleReader = true,

            // Писать могут разные потоки/шаги пайплайна — писателей несколько.
            SingleWriter = false,
        };

        _channel = Channel.CreateBounded<ProgressEventDto>(options);
    }

    public ChannelReader<ProgressEventDto> Reader => _channel.Reader;

    // С DropOldest TryWrite фактически всегда успевает (место освобождается выбросом
    // старого). false вернётся только если канал закрыт при остановке приложения.
    public bool TryWrite(ProgressEventDto progressEvent) => _channel.Writer.TryWrite(progressEvent);
}
