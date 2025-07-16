using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Messenger.Friends;

namespace Turbo.Messenger.Factories;
public class MessengerFriendsFactory(IServiceProvider serviceProvider) : IMessengerFriendsFactory
{
    public IMessengerFriendsManager Create(IMessenger messenger)
    {
        return ActivatorUtilities.CreateInstance<MessengerFriendsManager>(serviceProvider, messenger);
    }
}
