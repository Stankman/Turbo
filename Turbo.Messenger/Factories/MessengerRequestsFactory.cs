using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Messenger.Requests;

namespace Turbo.Messenger.Factories;

public class MessengerRequestsFactory(IServiceProvider _provider) : IMessengerRequestsFactory
{
    public IMessengerRequestsManager Create(IMessenger messenger)
    {
        return ActivatorUtilities.CreateInstance<MessengerRequestsManager>(_provider, messenger); 
    }
}
