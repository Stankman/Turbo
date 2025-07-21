using Turbo.Core.Game.Messenger.Constants;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendUpdate
{
    public int FriendId { get; set; }
    public IMessengerFriend? FriendData { get; set; }
    public FriendListUpdateActionEnum UpdateType { get; set; }
}
