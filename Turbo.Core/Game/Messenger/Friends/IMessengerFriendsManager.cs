using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendsManager : IComponent
{
    public IReadOnlyList<IMessengerFriend> Friends { get; }
    public IMessengerFriend? GetMessengerFriendAsync(int friendId);
    public Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AddMutualFriendsAsync(IPlayer friendPlayer);
    public Task<bool> DeleteFriendAsync(IPlayer friendPlayer);
    public void QueueFriendAdded(IMessengerFriend messengerFriend);
    public void QueueFriendUpdated(IMessengerFriend messengerFriend);
    public void QueueFriendRemoved(int friendId);
    public List<IMessengerFriendUpdate> DrainFriendUpdates();
    public void ClearFriendUpdateStates();
}
