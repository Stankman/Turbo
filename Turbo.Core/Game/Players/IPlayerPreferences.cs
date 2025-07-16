using System;
using System.Threading.Tasks;
using Turbo.Core.Database.Dtos;

namespace Turbo.Core.Game.Players;

public interface IPlayerPreferences
{
    bool isBlockingFriendRequests { get; set; }
    bool isAllowingFriendFollow { get; set; }
    DateTime CreatedAt { get; }
    DateTime UpdatedAt { get; }
    DateTime? DeletedAt { get; }
    public Task DisposeAsync();
}
