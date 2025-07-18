using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Packets.Messages;

namespace Turbo.Packets.Outgoing.FriendList;

public record NewFriendRequestMessage : IComposer
{
    public IMessengerRequest Request { get; init; }
}
