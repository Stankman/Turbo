using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger.Requests;

public interface IMessengerRequestsManager : IComponent
{
    public IReadOnlyList<IMessengerRequest> Requests { get; }
    public Task LoadFriendRequests();
    public Task<IMessengerRequest?> CreateFriendRequestAsync(IPlayer targetPlayer);
    public Task<IMessengerRequest?> GetFriendRequestAsync(int requestedByPlayerId);
    public Task DeleteFriendRequestAsync(int requestedByPlayerId);
    public Task ClearFriendRequestsAsync();
    public void InternalAddRequest(IMessengerRequest request);
}
