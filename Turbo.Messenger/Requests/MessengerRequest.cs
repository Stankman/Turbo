using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger.Requests;

namespace Turbo.Messenger.Requests;

public class MessengerRequest : IMessengerRequest
{
    public int PlayerEntityId { get; }
    public int RequestedPlayerEntityId { get; }

    public MessengerRequest(MessengerRequestEntity entity)
    {
        PlayerEntityId = entity.PlayerId;
        RequestedPlayerEntityId = entity.RequestedPlayerId;
    }
}
