using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;
using Turbo.Core.Networking.Game.Clients;
using Turbo.Core.PacketHandlers;
using Turbo.Core.Packets;
using Turbo.Packets.Incoming.FriendList;
using Turbo.Packets.Outgoing.FriendList;
using MessengerInitEventMessage = Turbo.Packets.Incoming.FriendList.MessengerInitMessage;
using MessengerInitMessage = Turbo.Packets.Outgoing.FriendList.MessengerInitMessage;

namespace Turbo.Main.PacketHandlers;

public class FriendListMessageHandler(
    IPacketMessageHub messageHub,
    IPlayerManager playerManager
) : IPacketHandlerManager
{
    public void Register()
    {
        messageHub.Subscribe<RequestFriendMessage>(this, OnRequestFriendMessage);
        messageHub.Subscribe<MessengerInitEventMessage>(this, OnMessengerInitMessage);
        messageHub.Subscribe<AcceptFriendMessage>(this, OnAcceptFriendMessage);
        messageHub.Subscribe<DeclineFriendMessage>(this, OnDeclineFriendMessage);
        messageHub.Subscribe<GetFriendRequestsMessage>(this, OnGetFriendRequestsMessage);
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

    private async Task OnDeclineFriendMessage(DeclineFriendMessage message, ISession session)
    {
    }

    private void OnMessengerInitMessage(MessengerInitEventMessage message, ISession session)
    {
        if (session.Player == null) return;

        session.Send(new MessengerInitMessage
        {
            userFriendLimit = 500,
            normalFriendLimit = 500,
            extendedFriendLimit = 3000
        });
    }

    private async Task OnRequestFriendMessage(RequestFriendMessage message, ISession session)
    {
        if (session.Player == null || string.IsNullOrWhiteSpace(message.PlayerName))
            return;

        var targetPlayer = await playerManager.GetPlayerByUsername(message.PlayerName);
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
}
