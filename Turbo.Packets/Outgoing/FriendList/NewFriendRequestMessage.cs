using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.FriendList;

public record NewFriendRequestMessage : IComposer
{
    public int PlayerId { get; init; }
    public string PlayerName { get; init; }
    public string PlayerFigure { get; init; }
}
