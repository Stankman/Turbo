using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;
using Turbo.Messenger.Friends;

namespace Turbo.Messenger.Factories;
public class MessengerFriendsFactory(
    IServiceProvider _provider) : IMessengerFriendsFactory
{
    public IMessengerFriendManager Create(IMessenger messenger)
    {
        return ActivatorUtilities.CreateInstance<MessengerFriendManager>(_provider, messenger);
    }

    public IMessengerFriend CreateMessengerFriend(MessengerFriendEntity messengerFriendEntity) 
    {
        var messengerFriendData = ActivatorUtilities.CreateInstance<MessengerFriendData>(_provider, messengerFriendEntity.FriendPlayerEntity);
        var messengerFriend = ActivatorUtilities.CreateInstance<MessengerFriend>(_provider, messengerFriendEntity, messengerFriendData);

        return messengerFriend;
    }
}
