using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Packets.Messages;
using Turbo.Core.Utilities;

namespace Turbo.Messenger;

public class Messenger : Component, IMessenger
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public IPlayerManager PlayerManager { get; }
    public IPlayer Player { get; }
    public int Id => Player.Id;
    public IMessengerFriendsManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }

    public Messenger(
        ILogger<IMessenger> _logger,
        IMessengerManager _messengerManager,
        IPlayerManager _playerManager,
        IPlayer _player,
        IMessengerFriendsFactory _messengerFriendsFactory,
        IMessengerRequestsFactory _messengerRequestsFactory)
    {
        Logger = _logger;
        MessengerManager = _messengerManager;
        PlayerManager = _playerManager;
        Player = _player;
        MessengerFriendsManager = _messengerFriendsFactory.Create(this);
        MessengerRequestsManager = _messengerRequestsFactory.Create(this);
    }

    protected override async Task OnInit()
    {
        try
        {
            await Task.WhenAll(
                MessengerFriendsManager.InitAsync().AsTask(),
                MessengerRequestsManager.InitAsync().AsTask()
            );

            MessengerManager.AddMessenger(this);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to initialize Messenger for PlayerId: {PlayerId}", Id);
        }
    }

    public bool HasSentRequestTo(int targetPlayerId)
    {
        return MessengerRequestsManager.Requests
            .Any(request => request.TargetPlayerEntityId == targetPlayerId);
    }

    public bool HasReceivedRequestFrom(int senderPlayerId)
    {
        return MessengerRequestsManager.Requests
            .Any(request => request.PlayerEntityId == senderPlayerId);
    }

    public List<IMessengerRequest> GetPendingRequests()
    {
        return MessengerRequestsManager.Requests.ToList();
    }

    public async Task<IMessengerRequest?> SendFriendRequest(IPlayer targetPlayer)
    {
        if (HasSentRequestTo(targetPlayer.Id))
            return null;

        if (HasReceivedRequestFrom(targetPlayer.Id))
            return null;

        return await MessengerRequestsManager.CreateFriendRequestAsync(targetPlayer);
    }

    public async Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AcceptFriend(int playerId)
    {
        var request = await MessengerRequestsManager.GetFriendRequestAsync(playerId);

        if (request == null)
        {
            Logger.LogWarning("No friend request found from player {PlayerId}", playerId);
            return (null, null);
        }

        var requestedByPlayer = PlayerManager.GetPlayerById(request.PlayerEntityId);

        if (requestedByPlayer == null)
        {
            await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);
            return (null, null);
        }

        var (playerMessengerFriend, friendMessengerFriend) = await MessengerFriendsManager.AddMutualFriendsAsync(requestedByPlayer);

        await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);

        return (playerMessengerFriend, friendMessengerFriend);
    }

    public async Task DeclineFriend(int playerId)
    {
        var request = await MessengerRequestsManager.GetFriendRequestAsync(playerId);

        if (request == null)
        {
            Logger.LogWarning("No friend request found from player {PlayerId}", playerId);
            return;
        }

        await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);
    }

    public async Task DeclineAll()
    {
        await MessengerRequestsManager.ClearFriendRequestsAsync();
    }

    public bool IsFriendWith(int playerId) => MessengerFriendsManager.Friends.Any(f => f.FriendPlayerEntityId == playerId);

    public bool HasPendingFriendRequestFrom(int playerId) => HasReceivedRequestFrom(playerId);

    public async Task SendComposer(IComposer composer)
    {
        var tasks = MessengerFriendsManager.Friends
            .Where(f => f.Friend.Status == PlayerStatusEnum.Online)
            .Select(f => f.Friend.Session.Send(composer));

        await Task.WhenAll(tasks);
    }

    protected override async Task OnDispose()
    {
        try
        {
            // Notify all friends that this player is going offline

            await MessengerFriendsManager.DisposeAsync();
            await MessengerRequestsManager.DisposeAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to remove Messenger for PlayerId: {PlayerId}", Id);
        }
    }
}