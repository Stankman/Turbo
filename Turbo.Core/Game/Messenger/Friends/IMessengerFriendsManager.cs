using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendsManager : IComponent
{
    public IReadOnlyList<IMessengerFriend> Friends { get; }
    public Task AddFriendAsync(IPlayer targetPlayer);
    public Task RemoveFriendAsync(IPlayer targetPlayer);
}
