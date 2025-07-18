using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Database.Entities.Messenger;

namespace Turbo.Database.Repositories.Messenger;

public interface IMessengerRequestsRepository : IBaseRepository<MessengerRequestEntity>
{
    public Task<List<MessengerRequestEntity>> FindPlayerRequestsAsync(int playerId);
    public Task<MessengerRequestEntity> CreateRequestAsync(int playerId, int targetPlayerId);
    public Task ClearFriendRequestsAsync(int playerId);
    public Task DeleteRequestAsync(int playerId, int targetPlayerId);
}
