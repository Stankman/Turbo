using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Messenger;

public class MessengerManager(
    ILogger<IMessengerManager> _logger,
    IMessengerFactory _messengerFactory,
    IServiceScopeFactory _serviceScopeFactory
) : Component, IMessengerManager
{
    private readonly ConcurrentDictionary<int, IMessenger> _messengers = new();
    protected override Task OnInit() => Task.CompletedTask;

    public IMessenger? GetMessengerForPlayer(IPlayer player)
    {
        if (player == null) return null;
        return _messengers.TryGetValue(player.Id, out var existingMessenger) ? existingMessenger : null;
    }

    public IMessenger? AddMessenger(IMessenger messenger)
    {
        if (messenger == null)
            return null;

        if (_messengers.TryAdd(messenger.Id, messenger))
            return messenger;

        _logger.LogWarning("Messenger for player {PlayerId} already exists", messenger.Id);
        return _messengers[messenger.Id];
    }

    public async Task RemoveMessenger(int playerId)
    {
        if (_messengers.TryRemove(playerId, out var messenger))
        {
            await messenger.DisposeAsync();
        }
    }

    private async Task RemoveAllMessengers()
    {
        if (_messengers.IsEmpty) return;

        var keys = _messengers.Keys.ToList();
        foreach (var playerId in keys)
        {
            await RemoveMessenger(playerId);
        }
    }

    protected override Task OnDispose() => RemoveAllMessengers();
}
