using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;

namespace Turbo.Core.Database.Factories.Messenger;

public interface IMessengerFriendsFactory
{
    public IMessengerFriendsManager Create(IMessenger messenger);
}
