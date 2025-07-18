using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;
using Turbo.Messenger.Friends;

namespace Turbo.Messenger.Factories;
public class MessengerFriendsFactory(
    IServiceProvider _provider) : IMessengerFriendsFactory
{
    public IMessengerFriendsManager Create(IMessenger messenger)
    {
        return ActivatorUtilities.CreateInstance<MessengerFriendsManager>(_provider, messenger);
    }

    public IMessengerFriend CreateMessengerFriend(MessengerFriendEntity messengerFriendEntity, IPlayer friendPlayer) 
    {
        return ActivatorUtilities.CreateInstance<MessengerFriend>(_provider, messengerFriendEntity, friendPlayer);
    }
}
