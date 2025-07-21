using System.Threading.Tasks;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Players;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.PacketHandlers;
using Turbo.Core.Packets;
using Turbo.Packets.Incoming.FriendList;
using Turbo.Packets.Outgoing.FriendList;
using MessengerInitEventMessage = Turbo.Packets.Incoming.FriendList.MessengerInitMessage;
using MessengerInitMessage = Turbo.Packets.Outgoing.FriendList.MessengerInitMessage;
using FriendListUpdateEventMessage = Turbo.Packets.Incoming.FriendList.FriendListUpdateMessage;
using FriendListUpdateMessage = Turbo.Packets.Outgoing.FriendList.FriendListUpdateMessage;
using System;

namespace Turbo.Main.PacketHandlers;

public class FriendListMessageHandler(
    IPacketMessageHub _messageHub,
    IPlayerManager _playerManager
) : IPacketHandlerManager
{
    public void Register()
    {
        _messageHub.Subscribe<AcceptFriendMessage>(this, OnAcceptFriendMessage);
        _messageHub.Subscribe<DeclineFriendMessage>(this, OnDeclineFriendMessage);
        _messageHub.Subscribe<FindNewFriendsMessage>(this, OnFindNewFriendMessage);
        _messageHub.Subscribe<FollowFriendMessage>(this, OnFollowFriendMessage);
        _messageHub.Subscribe<FriendListUpdateEventMessage>(this, OnFriendListUpdateMessage);
        _messageHub.Subscribe<GetFriendRequestsMessage>(this, OnGetFriendRequestsMessage);
        _messageHub.Subscribe<GetMessengerHistoryMessage>(this, OnGetMessengerHistoryMessage);
        _messageHub.Subscribe<HabboSearchMessage>(this, OnHabboSearchMessage);
        _messageHub.Subscribe<MessengerInitEventMessage>(this, OnMessengerInitMessage);
        _messageHub.Subscribe<RemoveFriendMessage>(this, OnRemoveFriendMessage);
        _messageHub.Subscribe<RequestFriendMessage>(this, OnRequestFriendMessage);
        _messageHub.Subscribe<SendMsgMessage>(this, OnSendMsgMessage);
        _messageHub.Subscribe<SendRoomInviteMessage>(this, OnSendRoomInviteMessage);
        _messageHub.Subscribe<VisitUserMessage>(this, OnVisitUserMessage);
    }

    private async Task OnAcceptFriendMessage(AcceptFriendMessage message, ISession session)
    {
        if (session.Player == null || message.Friends == null || message.Friends.Count == 0)
            return;

        foreach (var friendId in message.Friends)
        {
            if (!session.Player.Messenger.HasReceivedRequestFrom(friendId))
                continue;

            var (playerMessengerFriend, friendMessengerFriend) = await session.Player.Messenger.AcceptFriend(friendId);

            if (playerMessengerFriend != null)
            {
                await session.Send(new FriendListUpdateMessage
                {
                    AddedFriends = session.Player.Messenger.MessengerFriendsManager.GetFriendsByUpdateType(MessengerFriendUpdateStateEnum.Added)
                });
            }

            var friendPlayer = playerMessengerFriend?.Friend;

            if (friendPlayer?.Session != null && friendMessengerFriend != null)
            {
                await friendPlayer.Session.Send(new FriendListUpdateMessage
                {
                    AddedFriends = friendPlayer.Messenger.MessengerFriendsManager.GetFriendsByUpdateType(MessengerFriendUpdateStateEnum.Added)
                });
            }
        }
    }

    private void OnDeclineFriendMessage(DeclineFriendMessage message, ISession session)
    {
        if (session.Player == null)
            return;

        throw new NotImplementedException();

        //if(message.DeclineAll)
        //{

        //} else
        //{
        //    if(message.Friends == null || message.Friends.Count == 0)
        //        return;
        //}
    }

    private void OnFollowFriendMessage(FollowFriendMessage message, ISession session)
    {
        if (session.Player == null) return;

        if (message.PlayerId <= 0)
            return;

        throw new NotImplementedException();
    }

    private void OnFindNewFriendMessage(FindNewFriendsMessage message, ISession session) => throw new NotImplementedException();

    private void OnFriendListUpdateMessage(FriendListUpdateEventMessage message, ISession session)
    {
        if (session.Player == null)
            return;

        // Send the friend list updates
    }

    private void OnGetFriendRequestsMessage(GetFriendRequestsMessage message, ISession session)
    {
        if(session.Player == null)
            return;

        session.Send(new FriendRequestsMessage
        {
            Requests = session.Player.Messenger.GetPendingRequests()
        });
    }

    private void OnGetMessengerHistoryMessage(GetMessengerHistoryMessage message, ISession session) => throw new NotImplementedException();

    private void OnHabboSearchMessage(HabboSearchMessage message, ISession session)
    {
        if (session.Player == null || string.IsNullOrWhiteSpace(message.SearchQuery))
            return;

        throw new NotImplementedException();
        // Perform a search for players based on the search query
        //Must return a list of players that are friends and that are not friends
    }

    private void OnMessengerInitMessage(MessengerInitEventMessage message, ISession session)
    {
        if (session.Player == null) return;

        session.Send(new MessengerInitMessage
        {
            userFriendLimit = 500,
            normalFriendLimit = 500,
            extendedFriendLimit = 3000
            //Missing categories, not implemented yet
        });

        //session.Send(new FriendListFragmentMessage());
    }

    private void OnRemoveFriendMessage(RemoveFriendMessage message, ISession session)
    {
        if (session.Player == null) return;

        if (message.FriendIds == null || message.FriendIds.Count == 0)
            return;

        throw new NotImplementedException();

        //foreach (var friendId in message.FriendIds)
        //{
        //    // Find the friend by ID
        //    // If Friend does not exist, skip
        //    // Remove the friend from the list
        //}
    }

    private async Task OnRequestFriendMessage(RequestFriendMessage message, ISession session)
    {
        if (session.Player == null || string.IsNullOrWhiteSpace(message.PlayerName))
            return;

        var targetPlayer = _playerManager.GetPlayerByUsername(message.PlayerName);
        if (targetPlayer == null)
        {
            await session.Send(new MessengerErrorMessage
            {
                ErrorCode = MessengerErrorEnum.TargetNotFound
            });
            return;
        }

        if (targetPlayer.PlayerPreferences.isBlockingFriendRequests)
        {
            await session.Send(new MessengerErrorMessage
            {
                ErrorCode = MessengerErrorEnum.TargetBlockingRequests
            });
            return;
        }

        const int maxFriends = 500;
        if (session.Player.Messenger.MessengerFriendsManager.Friends.Count >= maxFriends)
        {
            await session.Send(new MessengerErrorMessage
            {
                ErrorCode = MessengerErrorEnum.FriendListFull
            });
            return;
        }

        if (targetPlayer.Messenger.MessengerFriendsManager.Friends.Count >= maxFriends)
        {
            await session.Send(new MessengerErrorMessage
            {
                ErrorCode = MessengerErrorEnum.TargetFriendListFull
            });
            return;
        }

        var request = await session.Player.Messenger.SendFriendRequest(targetPlayer);

        if (request != null && targetPlayer.Session != null)
        {
            await targetPlayer.Session.Send(new NewFriendRequestMessage
            {
                Request = request
            });
        }
    }

    private void OnSendMsgMessage(SendMsgMessage message, ISession session)
    {
        if (session.Player == null || message.ChatId <= 0 || string.IsNullOrWhiteSpace(message.Message))
            return;

        throw new NotImplementedException();
        //Find the friend by ID
        //If Friend does not exist, skip
        //Send the message to the friend
        //If friend is offline, store the message for later delivery
    }

    private void OnSendRoomInviteMessage(SendRoomInviteMessage message, ISession session)
    {
        if (session.Player == null) return;

        if (String.IsNullOrWhiteSpace(message.Message)) return;

        if (message.FriendIds == null || message.FriendIds.Count == 0)
            return;

        throw new NotImplementedException();

        //foreach (var friendId in message.FriendIds)
        //{
        //    //Find the friend by ID
        //    //If Friend does not exist, skip

        //    //Messenger Sent Invite even if offline, he will receive it when he logs in
        //}
    }

    private void OnVisitUserMessage(VisitUserMessage message, ISession session) => throw new NotImplementedException();
}
