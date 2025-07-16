namespace Turbo.Core.Game.Messenger.Requests;

public interface IMessengerRequest
{
    public int PlayerEntityId { get; }
    public int RequestedPlayerEntityId { get; }
}
