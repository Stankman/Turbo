using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger;

public interface IMessengerEvents : IComponent
{
    public Task OnAddedNewFriendsEvent(IPlayer player);
    public Task OnRemovedFriendsEvent(IPlayer player, List<int> removedFriends);
    public Task OnPlayerDetailsUpdateEvent(IPlayer player, IMessengerFriendData update);
    public Task OnPlayerStatusUpdateEvent(IPlayer player, PlayerStatusEnum update);
}
