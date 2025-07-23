using Turbo.Core.Game.Messenger.Constants;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendUpdate
{
    public int FriendId { get; set; }
    public IMessengerFriendData? FriendData { get; set; }
    public int CategoryId { get; set; }
    public MessengerFriendRelationEnum RelationType { get; set; }
    public FriendListUpdateActionEnum UpdateType { get; set; }
}
