using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;

namespace Turbo.Messenger.Friends;

public class MessengerFriend(
    MessengerFriendEntity _entity,
    IPlayer _friendPlayer) : IMessengerFriend
{
    public int PlayerEntityId { get; } = _entity.PlayerId;
    public int FriendPlayerEntityId { get; } = _entity.FriendPlayerId;
    public IPlayer Friend { get; } = _friendPlayer;
    public int? MessengerCategoryEntityId { get; } = _entity.MessengerCategoryEntityId;
    public MessengerFriendRelationEnum RelationType { get; } = _entity.RelationType;
}
