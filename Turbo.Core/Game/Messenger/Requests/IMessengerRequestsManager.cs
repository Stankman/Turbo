using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger.Requests;

public interface IMessengerRequestsManager : IComponent
{
    IReadOnlyList<IMessengerRequest> Requests { get; }
    Task LoadFriendRequests();
    Task CreateFriendRequestAsync(IPlayer targetPlayer);
    Task ClearFriendRequestsAsync();
    Task DeleteFriendRequestAsync(IPlayer requestedByPlayer);
}
