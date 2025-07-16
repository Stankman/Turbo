using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Database.Entities.Players;
using Turbo.Database.Context;

namespace Turbo.Database.Repositories.Player;

public class PlayerPreferencesRepository(IEmulatorContext _context) : IPlayerPreferencesRepository
{
    public async Task<PlayerPreferencesEntity> CreateDefaultPreferences(PlayerPreferencesDto playerPreferencesDto)
    {
        var entity = new PlayerPreferencesEntity
        {
            PlayerId = playerPreferencesDto.PlayerEntityId,
            BlockFriendRequests = playerPreferencesDto.BlockFriendRequests,
            AllowFriendFollow = playerPreferencesDto.AllowFriendsFollow
        };

        _context.PlayerPreferences.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<PlayerPreferencesEntity> FindAsync(int id)
    {
        return await _context.PlayerPreferences
            .FindAsync(id);
    }
}