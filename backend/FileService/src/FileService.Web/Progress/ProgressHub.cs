using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using FileService.Contracts.Features.Progress;
using Microsoft.AspNetCore.SignalR;

namespace FileService.Web.Progress;

/// <summary>
/// SignalR-хаб realtime-прогресса. Клиент подключается к /hubs/progress и вызывает
/// Subscribe(assetId) — сервер заводит его в группу этого видео И сразу шлёт текущее
/// состояние. Дальнейшая рассылка идёт в группу (см. ProgressBroadcastConsumer).
/// Группы SignalR заменяют ручной SSE-реестр подписчиков.
/// </summary>
public class ProgressHub : Hub
{
    public const string ClientMethod = "ReceiveProgress";

    private readonly InitialProcess _initial;

    public ProgressHub(InitialProcess initial)
    {
        _initial = initial;
    }

    public async Task Subscribe(Guid mediaAssetId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(mediaAssetId));
        
        Result<ProgressEventDto, Error> initial = await _initial.InitialProcessEvent(mediaAssetId);
        if (initial.IsSuccess)
            await Clients.Caller.SendAsync(ClientMethod, initial.Value);
    }

    public Task Unsubscribe(Guid mediaAssetId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(mediaAssetId));

    public static string GroupName(Guid mediaAssetId) => $"progress:{mediaAssetId}";
}
