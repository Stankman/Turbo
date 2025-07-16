using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Database.Context;

namespace Turbo.Database.Repositories.Messenger;

public class MessengerFriendsRepository(IEmulatorContext _context) : IMessengerFriendsRepository
{
    public async Task<MessengerFriendEntity> FindAsync(int id)
    {
        return await _context.MessengerFriends
            .FindAsync(id);
    }

    public async Task<List<MessengerFriendEntity>> FindAllByPlayerIdAsync(int playerId)
    {
        return await _context.MessengerFriends
            .Where(entity => entity.PlayerId == playerId)
            .ToListAsync();
    }

    public async Task<MessengerFriendEntity> AddFriendAsync(int playerId, int friendPlayerId)
    {
        var entity = new MessengerFriendEntity
        {
            PlayerId = playerId,
            FriendPlayerId = friendPlayerId
        };
        _context.MessengerFriends.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task RemoveFriendAsync(int playerId, int friendPlayerId)
    {
        var entity = await _context.MessengerFriends
            .FirstOrDefaultAsync(f => f.PlayerId == playerId && f.FriendPlayerId == friendPlayerId);

        if (entity != null)
        {
            _context.MessengerFriends.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
