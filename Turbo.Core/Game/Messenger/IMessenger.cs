using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger;

public interface IMessenger : IComponent
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public int Id { get; }
    public IMessengerFriendsManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }
    public bool HasRequested(IPlayer targetPlayer);
}
