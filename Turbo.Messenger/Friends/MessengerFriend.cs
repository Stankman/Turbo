using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;

namespace Turbo.Messenger.Friends;

public class MessengerFriend(
    MessengerFriendEntity _entity,
    IMessengerFriendData _messengerFriendData) : IMessengerFriend
{
    public int Id { get; } = _entity.FriendPlayerId;
    public IMessengerFriendData Friend { get; set; } = _messengerFriendData;
    public int? MessengerCategoryEntityId { get; } = _entity.MessengerCategoryEntityId;
    public MessengerFriendRelationEnum RelationType { get; } = _entity.RelationType;
}
