using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Players;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriend
{
    public int PlayerEntityId { get; }
    public IPlayer Friend { get; }
    public int FriendPlayerEntityId { get; }
    public int? MessengerCategoryEntityId { get; }
    public MessengerFriendRelationEnum RelationType { get; }
}
