using System.Collections.Generic;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.FriendList;
public record FriendListUpdateMessage : IComposer
{
    //Send Categories as well
    public List<IMessengerFriend>? AddedFriends { get; init; }
    public List<IMessengerFriend>? UpdatedFriends { get; init; }
    public List<int>? RemovedFriends { get; init; }
}
