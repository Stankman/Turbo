using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Utilities;

namespace Turbo.Messenger;

public class Messenger : Component, IMessenger
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public IPlayerManager PlayerManager { get; }
    private IPlayer Player { get; }
    public int Id => Player.Id;
    public IMessengerFriendManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }
    public IMessengerEvents MessengerEvents { get; }
    public List<IMessengerRequest> PendingRequests => MessengerRequestsManager.Requests.ToList();
    public List<IMessengerFriend> Friends => MessengerFriendsManager.Friends.ToList();

    public event EventHandler? AddedNewFriendsEvent;
    public event EventHandler<List<int>>? RemovedFriendsEvent;

    public Messenger(
        ILogger<IMessenger> _logger,
        IMessengerManager _messengerManager,
        IPlayerManager _playerManager,
        IPlayer _player,
        IMessengerFriendsFactory _messengerFriendsFactory,
        IMessengerRequestsFactory _messengerRequestsFactory,
        IMessengerEventsFactory _messengerEventsFactory)
    {
        Logger = _logger;
        MessengerManager = _messengerManager;
        PlayerManager = _playerManager;
        Player = _player;
        MessengerFriendsManager = _messengerFriendsFactory.Create(this);
        MessengerRequestsManager = _messengerRequestsFactory.Create(this);
        MessengerEvents = _messengerEventsFactory.Create(Player);
    }

    protected override async Task OnInit()
    {
        try
        {
            await Task.WhenAll(
                MessengerFriendsManager.InitAsync().AsTask(),
                MessengerRequestsManager.InitAsync().AsTask(),
                MessengerEvents.InitAsync().AsTask()
            );

            MessengerManager.AddMessenger(this);

            await MessengerEvents.OnPlayerStatusUpdateEvent(Player, Player.Status);
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

    public bool HasPendingFriendRequestFrom(int senderPlayerId)
    {
        return MessengerRequestsManager.Requests
            .Any(request => request.PlayerEntityId == senderPlayerId);
    }

    public async Task<IMessengerRequest?> SendFriendRequest(IPlayer targetPlayer)
    {
        if (HasSentRequestTo(targetPlayer.Id))
            return null;

        if (HasPendingFriendRequestFrom(targetPlayer.Id))
            return null;

        return await MessengerRequestsManager.CreateFriendRequestAsync(targetPlayer);
    }

    public async Task AcceptFriendRequests(List<int> playerIds)
    {
        foreach(var playerId in playerIds)
        {
            if (!HasPendingFriendRequestFrom(playerId)) continue;

            var request = await MessengerRequestsManager.GetFriendRequestAsync(playerId);

            if (request == null) continue;

            var requestedByPlayer = await PlayerManager.GetOfflinePlayerById(request.PlayerEntityId);

            if (requestedByPlayer == null)
            {
                await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);
                continue;
            }

            var (playerMessengerFriend, friendMessengerFriend) = await MessengerFriendsManager.AddMutualFriendsAsync(requestedByPlayer);

            await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);
        }

        AddedNewFriendsEvent?.Invoke(this, EventArgs.Empty);
    }

    public async Task DeleteFriends(List<int> friendIds)
    {
        List<int> removedFriendsIds = new List<int>();

        foreach (var friendId in friendIds)
        {
            if (!IsFriendWith(friendId)) continue;

            var friend = MessengerFriendsManager.GetFriendById(friendId);

            if (friend == null) continue;

            await MessengerFriendsManager.DeleteFriendAsync(friendId);

            removedFriendsIds.Add(friendId);
        }

        RemovedFriendsEvent?.Invoke(this, removedFriendsIds);
    }

    public async Task DeclineFriendRequests(List<int> friendIds)
    {
        foreach(var playerId in friendIds)
        {
            if (!HasPendingFriendRequestFrom(playerId)) continue;

            var request = await MessengerRequestsManager.GetFriendRequestAsync(playerId);

            if (request == null) continue;

            var requestedByPlayer = await PlayerManager.GetOfflinePlayerById(request.PlayerEntityId);

            if (requestedByPlayer == null)
            {
                await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);
                continue;
            }

            await MessengerRequestsManager.DeleteFriendRequestAsync(playerId);

            //Invoke event for request declined
        }
    }

    public async Task DeclineAll()
    {
        await MessengerRequestsManager.ClearFriendRequestsAsync();
    }

    public bool IsFriendWith(int playerId) => MessengerFriendsManager.Friends.Any(f => f.Id == playerId);

    public IMessengerFriend? GetFriendById(int friendPlayerId)
    {
        return MessengerFriendsManager.GetFriendById(friendPlayerId);
    }

    protected override async Task OnDispose()
    {
        try
        {
            await MessengerEvents.DisposeAsync();
            await MessengerFriendsManager.DisposeAsync();
            await MessengerRequestsManager.DisposeAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to remove Messenger for PlayerId: {PlayerId}", Id);
        }
    }
}