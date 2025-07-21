using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;

namespace Turbo.Messenger.Friends;

public class MessengerFriendUpdate : IMessengerFriendUpdate
{
    public int FriendId { get; set; }
    public IMessengerFriend? FriendData { get; set; }
    public FriendListUpdateActionEnum UpdateType { get; set; }
}
