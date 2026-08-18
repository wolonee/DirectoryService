using FileService.Contracts.Features.Progress;
using FileService.Core.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace FileService.Web.Progress;

/// <summary>
/// Фоновый сервис-мост: читает очередь прогресса и рассылает события подписчикам
/// через SignalR. Работает всё время жизни приложения (BackgroundService).
///
/// IHubContext позволяет пушить в хаб ИЗВНЕ хаба (мы не внутри Hub-метода, а в фоне).
/// Падение рассылки одному событию не должно ронять цикл — оборачиваем в try/catch.
/// </summary>
public class ProgressBroadcastConsumer : BackgroundService
{
    private readonly IProgressQueue _queue;
    private readonly IHubContext<ProgressHub> _hub;
    private readonly ILogger<ProgressBroadcastConsumer> _logger;

    public ProgressBroadcastConsumer(
        IProgressQueue queue,
        IHubContext<ProgressHub> hub,
        ILogger<ProgressBroadcastConsumer> logger)
    {
        _queue = queue;
        _hub = hub;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ReadAllAsync крутится, пока в очереди есть события; когда пусто — асинхронно
        // ждёт следующего, не сжигая CPU. Выходит при остановке приложения.
        await foreach (ProgressEventDto progressEvent in _queue.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await _hub.Clients
                    .Group(ProgressHub.GroupName(progressEvent.MediaAssetId))
                    .SendAsync(ProgressHub.ClientMethod, progressEvent, stoppingToken);
            }
            catch (Exception ex)
            {
                // Рассылка — побочный канал: сбой доставки логируем, но обработку видео
                // (которая уже давно ушла дальше) это не касается.
                _logger.LogError(
                    ex,
                    "Failed to broadcast progress for VideoAssetId: {VideoAssetId}",
                    progressEvent.MediaAssetId);
            }
        }
    }
}
