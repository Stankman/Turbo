using System.Collections.Generic;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.FriendList;

public record FriendListFragmentMessage : IComposer
{
    public required List<List<IMessengerFriend>> FriendListFragments { get; init; }
}