using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Players;

namespace Turbo.Core.Database.Factories.Messenger;
public interface IMessengerEventsFactory
{
    public IMessengerEvents Create(IPlayer player);
}
