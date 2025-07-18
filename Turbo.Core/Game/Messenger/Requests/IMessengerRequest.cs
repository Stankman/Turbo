using Turbo.Core.Database.Entities.Players;

namespace Turbo.Core.Game.Messenger.Requests;

public interface IMessengerRequest
{
    public PlayerEntity PlayerEntity { get; }
    public int PlayerEntityId { get; }
    public PlayerEntity TargetPlayerEntity { get; }
    public int TargetPlayerEntityId { get; }
}
