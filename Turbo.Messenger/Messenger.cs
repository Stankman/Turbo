using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Packets.Messages;
using Turbo.Core.Utilities;
using Turbo.Packets.Outgoing.FriendList;

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
        ILogger<IMessenger> logger,
        IMessengerManager messengerManager,
        IPlayerManager playerManager,
        IPlayer player,
        IMessengerFriendsFactory messengerFriendsFactory,
        IMessengerRequestsFactory messengerRequestsFactory)
    {
        Logger = logger;
        MessengerManager = messengerManager;
        Player = player;
        MessengerFriendsManager = messengerFriendsFactory.Create(this);
        MessengerRequestsManager = messengerRequestsFactory.Create(this);
    }

    protected override async Task OnInit()
    {
        try
        {
            await MessengerFriendsManager.InitAsync();
            await MessengerRequestsManager.InitAsync();
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

        var requestedByPlayer = await PlayerManager.GetPlayerById(request.PlayerEntityId);

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

    public void SendComposer(IComposer composer)
    {
        foreach (var friend in MessengerFriendsManager.Friends)
        {
            var friendPlayer = friend.Friend;

            if (friendPlayer.Status.Equals(PlayerStatusEnum.Online))
            {
                friendPlayer.Session.Send(composer);
            }
        }
    }

    protected override async Task OnDispose()
    {
        await MessengerFriendsManager.DisposeAsync();
        await MessengerRequestsManager.DisposeAsync();
    }
}