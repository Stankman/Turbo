using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Database.Entities.Players;

namespace Turbo.Database.Repositories.Player;

public interface IPlayerPreferencesRepository : IBaseRepository<PlayerPreferencesEntity>
{
    Task<PlayerPreferencesEntity> CreateDefaultPreferences(PlayerPreferencesDto playerPreferencesDto);
}
