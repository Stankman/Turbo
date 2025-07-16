using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;

namespace Turbo.Core.Database.Factories.Messenger;
public interface IMessengerRequestsFactory
{
    public IMessengerRequestsManager Create(IMessenger messenger);
}
