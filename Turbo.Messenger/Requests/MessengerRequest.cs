using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Game.Messenger.Requests;

namespace Turbo.Messenger.Requests;

public class MessengerRequest(
    MessengerRequestEntity _messengerRequestEntity) : IMessengerRequest
{
    public PlayerEntity PlayerEntity { get; } = _messengerRequestEntity.PlayerEntity;
    public int PlayerEntityId { get; } = _messengerRequestEntity.PlayerId;
    public PlayerEntity TargetPlayerEntity { get; } = _messengerRequestEntity.RequestedPlayerEntity;
    public int TargetPlayerEntityId { get; } = _messengerRequestEntity.RequestedPlayerId;
}
