using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Database.Entities.Messenger;

namespace Turbo.Database.Repositories.Messenger;

public interface IMessengerFriendsRepository : IBaseRepository<MessengerFriendEntity>
{
    public Task<List<MessengerFriendEntity>> FindAllByPlayerIdAsync(int playerId);
    public Task<MessengerFriendEntity> AddFriendAsync(int playerId, int friendPlayerId);
    public Task RemoveFriendAsync(int playerId, int friendPlayerId);
}
