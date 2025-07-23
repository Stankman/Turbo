using Turbo.Core.Game.Messenger.Constants;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriend
{
    public IMessengerFriendData Friend { get; set; }
    public int Id { get; }
    public int? MessengerCategoryEntityId { get; }
    public MessengerFriendRelationEnum RelationType { get; }
}
