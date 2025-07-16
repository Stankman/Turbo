using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger.Friends;

namespace Turbo.Messenger.Friends;

public class MessengerFriend : IMessengerFriend
{
    public int PlayerEntityId { get; }
    public int FriendPlayerEntityId { get; }
    public int? MessengerCategoryEntityId { get; }
    public int Id { get; }
    public string FriendName { get; }
    public string FriendLook { get; }
    public int RelationType { get; }

    public MessengerFriend(MessengerFriendEntity entity)
    {
        PlayerEntityId = entity.PlayerId;
        FriendPlayerEntityId = entity.FriendPlayerId;
        MessengerCategoryEntityId = entity.MessengerCategoryEntityId;
        Id = entity.Id;
        FriendName = entity.FriendPlayerEntity?.Name ?? string.Empty;
        FriendLook = entity.FriendPlayerEntity?.Figure ?? string.Empty;
        RelationType = (int)entity.RelationType;
    }
}
