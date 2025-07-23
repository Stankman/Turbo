using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Utilities;
using Turbo.Packets.Outgoing.FriendList;

namespace Turbo.Messenger;

public class MessengerEvents (
    ILogger<IMessengerEvents> _logger,
    IPlayer _player) : Component, IMessengerEvents
{
    private IPlayer Player { get; } = _player;

    protected override async Task OnInit()
    {
        Player.PlayerDetails.PlayerDetailsUpdateEvent += async (_, update) => await OnPlayerDetailsUpdateEvent(Player, (IMessengerFriendData)update);
        Player.PlayerDetails.StatusUpdateEvent += async (_, update) => await OnPlayerStatusUpdateEvent(Player, (PlayerStatusEnum)update);
        Player.Messenger.AddedNewFriendsEvent += async (_, _) => await OnAddedNewFriendsEvent(Player);
        Player.Messenger.RemovedFriendsEvent += async (_, removedFriends) => await OnRemovedFriendsEvent(Player, (List<int>)removedFriends);

        await Task.CompletedTask;
    }

    public async Task OnRemovedFriendsEvent(IPlayer player, List<int> removedFriends)
    {
        foreach(var removedFriend in removedFriends)
        {
            var friendPlayer = player.PlayerManager.GetPlayerById(removedFriend);

            if (friendPlayer == null || friendPlayer.Session == null || friendPlayer.Status.Equals(PlayerStatusEnum.Offline)) continue;

            await friendPlayer.Session.Send(new FriendListUpdateMessage
            {
                FriendListUpdate = friendPlayer.Messenger.MessengerFriendsManager.GetFriendListUpdates()
            });
        }

        await player.Session.Send(new FriendListUpdateMessage
        {
            FriendListUpdate = player.Messenger.MessengerFriendsManager.GetFriendListUpdates()
        });
    }

    public async Task OnAddedNewFriendsEvent(IPlayer player)
    {
        foreach (var friend in player.Messenger.Friends)
        {
            var friendPlayer = player.PlayerManager.GetPlayerById(friend.Id);

            if (friendPlayer == null || friendPlayer.Session == null || friendPlayer.Status.Equals(PlayerStatusEnum.Offline)) continue;

            await friendPlayer.Session.Send(new FriendListUpdateMessage
            {
                FriendListUpdate = friendPlayer.Messenger.MessengerFriendsManager.GetFriendListUpdates()
            });
        }

        await player.Session.Send(new FriendListUpdateMessage
        {
            FriendListUpdate = player.Messenger.MessengerFriendsManager.GetFriendListUpdates()
        });
    }

    public async Task OnPlayerDetailsUpdateEvent(IPlayer player, IMessengerFriendData update)
    {
        foreach (var friend in player.Messenger.Friends)
        {
            var friendPlayer = player.PlayerManager.GetPlayerById(friend.Id);

            if (friendPlayer == null || friendPlayer.Status.Equals(PlayerStatusEnum.Offline)) continue;

            var me = friendPlayer.Messenger.GetFriendById(player.Id);

            if (me == null) continue;

            me.Friend = update;

            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendUpdated(me);
        }

        await Task.CompletedTask;
    }

    public async Task OnPlayerStatusUpdateEvent(IPlayer player, PlayerStatusEnum update)
    {
        foreach (var friend in player.Messenger.Friends)
        {
            var friendPlayer = player.PlayerManager.GetPlayerById(friend.Id);

            if (friendPlayer == null || friendPlayer.Status.Equals(PlayerStatusEnum.Offline)) continue;

            var me = friendPlayer.Messenger.GetFriendById(player.Id);

            if (me == null) continue;

            me.Friend.Status = update;

            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendUpdated(me);

            await friendPlayer.Session.Send(new FriendListUpdateMessage
            {
                FriendListUpdate = friendPlayer.Messenger.MessengerFriendsManager.GetFriendListUpdates()
            });
        }
    }

    protected override async Task OnDispose()
    {
        Player.PlayerDetails.PlayerDetailsUpdateEvent -= async (_, update) => await OnPlayerDetailsUpdateEvent(Player, (IMessengerFriendData)update);
        Player.PlayerDetails.StatusUpdateEvent -= async (_, update) => await OnPlayerStatusUpdateEvent(Player, (PlayerStatusEnum)update);
        Player.Messenger.AddedNewFriendsEvent -= async (_, _) => await OnAddedNewFriendsEvent(Player);
        Player.Messenger.RemovedFriendsEvent -= async (_, removedFriends) => await OnRemovedFriendsEvent(Player, (List<int>)removedFriends);

        await Task.CompletedTask;
    }
}
