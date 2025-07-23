using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Players;

namespace Turbo.Messenger.Factories;
public class MessengerEventsFactory(IServiceProvider _provider) : IMessengerEventsFactory
{
    public IMessengerEvents Create(IPlayer player)
    {
        return ActivatorUtilities.CreateInstance<MessengerEvents>(_provider, player);
    }
}
