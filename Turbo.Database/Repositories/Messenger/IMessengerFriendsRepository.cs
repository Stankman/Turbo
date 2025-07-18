using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Database.Entities.Messenger;

namespace Turbo.Database.Repositories.Messenger;

public interface IMessengerFriendsRepository : IBaseRepository<MessengerFriendEntity>
{
    public Task<List<MessengerFriendEntity>> FindAllByPlayerIdAsync(int playerId);
    public Task<(MessengerFriendEntity PlayerSide, MessengerFriendEntity FriendSide)> AddMutualFriendsAsync(int playerId, int friendId);
    public Task DeleteMutualFriendsAsync(int playerId, int friendId);
}
