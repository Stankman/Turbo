using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Core.Game.Messenger.Constants;
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
            .Include(entity => entity.PlayerEntity)
            .Include(entity => entity.FriendPlayerEntity)
            .ToListAsync();
    }

    public async Task<(MessengerFriendEntity PlayerSide, MessengerFriendEntity FriendSide)> AddMutualFriendsAsync(int playerId, int friendPlayerId)
    {
        var existing = await _context.MessengerFriends
            .Where(f =>
                (f.PlayerId == playerId && f.FriendPlayerId == friendPlayerId) ||
                (f.PlayerId == friendPlayerId && f.FriendPlayerId == playerId))
            .ToListAsync();

        var playerSide = existing.FirstOrDefault(f => f.PlayerId == playerId);
        var friendSide = existing.FirstOrDefault(f => f.PlayerId == friendPlayerId);

        if (playerSide == null)
        {
            playerSide = new MessengerFriendEntity { PlayerId = playerId, FriendPlayerId = friendPlayerId };
            _context.MessengerFriends.Add(playerSide);
        }

        if (friendSide == null)
        {
            friendSide = new MessengerFriendEntity { PlayerId = friendPlayerId, FriendPlayerId = playerId };
            _context.MessengerFriends.Add(friendSide);
        }

        await _context.SaveChangesAsync();

        return (playerSide, friendSide);
    }

    public async Task DeleteMutualFriendsAsync(int playerId, int friendPlayerId)
    {
        var friendsToDelete = await _context.MessengerFriends
            .Where(f =>
                (f.PlayerId == playerId && f.FriendPlayerId == friendPlayerId) ||
                (f.PlayerId == friendPlayerId && f.FriendPlayerId == playerId))
            .ToListAsync();
        _context.MessengerFriends.RemoveRange(friendsToDelete);
        await _context.SaveChangesAsync();
    }
}
