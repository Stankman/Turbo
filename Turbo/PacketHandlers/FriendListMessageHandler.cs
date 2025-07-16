using System;
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
    }

    private void OnAcceptFriendMessage(AcceptFriendMessage message, ISession session)
    {
        if(session.Player == null || message.Friends == null || message.Friends.Count == 0)
            return;

    }

    private async Task OnDeclineFriendMessage(DeclineFriendMessage message, ISession session)
    {
        if (session.Player == null)
            return;

        if (!message.DeclineAll && message.Friends == null || message.Friends.Count == 0)
            return;

        if (message.DeclineAll)
        {
            await session.Player.Messenger.MessengerRequestsManager.ClearFriendRequestsAsync();
        }
        else
        {
            foreach (var friendId in message.Friends)
            {
                var friendPlayer = await playerManager.GetPlayerById(friendId);
                if (friendPlayer == null)
                {
                    continue;
                }

                await session.Player.Messenger.MessengerRequestsManager.DeleteFriendRequestAsync(friendPlayer);
            }
        }
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

        // TODO: Replace with emulator settings for max friends and check for special permissions
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

        //Probably scripted, but we can check if the player has already requested the target player
        if (session.Player.Messenger.HasRequested(targetPlayer))
        {
            return;
        }

        // TODO: Allow plugins to cancel the friend request

        // If player is not online, still make the friend request
        await session.Player.Messenger.MessengerRequestsManager.CreateFriendRequestAsync(targetPlayer);

        if (targetPlayer.Session != null)
        {
            await targetPlayer.Session.Send(new NewFriendRequestMessage
            {
                PlayerId = session.Player.Id,
                PlayerName = session.Player.Name,
                PlayerFigure = session.Player.Figure
            });
        }
    }
}
