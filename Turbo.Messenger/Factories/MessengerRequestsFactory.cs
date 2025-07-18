using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Messenger.Requests;

namespace Turbo.Messenger.Factories;

public class MessengerRequestsFactory(IServiceProvider _provider) : IMessengerRequestsFactory
{
    public IMessengerRequestsManager Create(IMessenger messenger)
    {
        return ActivatorUtilities.CreateInstance<MessengerRequestsManager>(_provider, messenger); 
    }

    public IMessengerRequest CreateMessengerRequest(MessengerRequestEntity requestEntity)
    {
        return ActivatorUtilities.CreateInstance<MessengerRequest>(_provider, requestEntity);
    }
}
