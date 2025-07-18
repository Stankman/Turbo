using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;

namespace Turbo.Core.Database.Factories.Messenger;
public interface IMessengerRequestsFactory
{
    public IMessengerRequestsManager Create(IMessenger messenger);
    public IMessengerRequest CreateMessengerRequest(MessengerRequestEntity messengerRequestEntity);
}
