using System.Collections.Generic;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.FriendList;
public record FriendListUpdateMessage : IComposer
{
    //Send Categories as well
    public required List<IMessengerFriendUpdate> FriendListUpdate { get; init; }
}
