using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Turbo.Core.Database.Entities.Messenger;
using Turbo.Database.Context;

namespace Turbo.Database.Repositories.Messenger;

public class MessengerRequestsRepository(IEmulatorContext _context) : IMessengerRequestsRepository
{
    public async Task<MessengerRequestEntity> FindAsync(int id)
    {
        return await _context.MessengerRequests
            .FindAsync(id);
    }

    public async Task<List<MessengerRequestEntity>> FindPlayerRequestsAsync(int playerId)
    {
        return await _context.MessengerRequests
            .Where(entity => entity.RequestedPlayerId == playerId)
            .ToListAsync();
    }

    public async Task<MessengerRequestEntity> CreateRequestAsync(int playerId, int requestedPlayerId)
    {
        var entity = new MessengerRequestEntity
        {
            PlayerId = playerId,
            RequestedPlayerId = requestedPlayerId
        };
        
        _context.MessengerRequests.Add(entity);
        
        await _context.SaveChangesAsync();

        return entity;
    }

    public async Task DeleteRequestAsync(int playerId, int targetPlayerId)
    {
        var entity = await _context.MessengerRequests
            .FirstOrDefaultAsync(r => r.PlayerId == playerId && r.RequestedPlayerId == targetPlayerId);

        if (entity != null)
        {
            _context.MessengerRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task ClearFriendRequestsAsync(int playerId)
    {
        var entities = await _context.MessengerRequests
            .Where(request => request.RequestedPlayerId == playerId)
            .ToListAsync();

        if (entities.Any())
        {
            _context.MessengerRequests.RemoveRange(entities);
            await _context.SaveChangesAsync();
        }
    }
}
