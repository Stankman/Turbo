using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;

namespace Turbo.Core.Database.Factories.Messenger;

public interface IMessengerFriendsFactory
{
    public IMessengerFriendsManager Create(IMessenger messenger);
    public IMessengerFriend CreateMessengerFriend(MessengerFriendEntity messengerFriendEntity, IPlayer friendPlayer);
}
