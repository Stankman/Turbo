using System;
using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Game.Players;
using Turbo.Core.Storage;

namespace Turbo.Players;

public class PlayerPreferences(
    PlayerPreferencesEntity _playerPreferencesEntity,
    IStorageQueue _storageQueue) : IPlayerPreferences
{
    public async Task DisposeAsync()
    {
        await _storageQueue.SaveEntityNow(_playerPreferencesEntity);
    }

    public bool isBlockingFriendRequests
    {
        get => _playerPreferencesEntity.BlockFriendRequests;
        set
        {
            _playerPreferencesEntity.BlockFriendRequests = value;
            _storageQueue.Add(_playerPreferencesEntity);
        }
    }

    public bool isAllowingFriendFollow
    {
        get => _playerPreferencesEntity.AllowFriendFollow;
        set
        {
            _playerPreferencesEntity.AllowFriendFollow = value;
            _storageQueue.Add(_playerPreferencesEntity);
        }
    }

    public DateTime CreatedAt => _playerPreferencesEntity.CreatedAt;

    public DateTime UpdatedAt => _playerPreferencesEntity.UpdatedAt;

    public DateTime? DeletedAt => _playerPreferencesEntity.DeletedAt;
}
