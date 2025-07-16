using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Messenger;

public class Messenger : Component, IMessenger
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public int Id { get; }
    public IMessengerFriendsManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }

    public Messenger(
        ILogger<IMessenger> logger,
        IMessengerManager messengerManager,
        IPlayer player,
        IMessengerFriendsFactory messengerFriendsFactory,
        IMessengerRequestsFactory messengerRequestsFactory)
    {
        Logger = logger;
        MessengerManager = messengerManager;
        Id = player.Id;
        MessengerFriendsManager = messengerFriendsFactory.Create(this);
        MessengerRequestsManager = messengerRequestsFactory.Create(this);
    }

    protected override async Task OnInit()
    {
        await MessengerFriendsManager.InitAsync();
        await MessengerRequestsManager.InitAsync();
        await MessengerManager.AddMessenger(this);
    }

    public bool HasRequested(IPlayer targetPlayer)
    {
        return MessengerRequestsManager.Requests
            .Any(request => request.RequestedPlayerEntityId == targetPlayer.Id);
    }

    protected override async Task OnDispose()
    {
        await MessengerFriendsManager.DisposeAsync();
        await MessengerFriendsManager.DisposeAsync();
    }
}