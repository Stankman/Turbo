using System.Collections.Generic;
using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendManager : IComponent
{
    public IReadOnlyCollection<IMessengerFriend> Friends { get; }
    public IMessengerFriend? GetFriendById(int friendId);
    public List<List<IMessengerFriend>> GetFriendsFragments(int fragmentSize);
    public Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AddMutualFriendsAsync(IPlayer friendPlayer);
    public Task<bool> DeleteFriendAsync(int friendId);
    public void QueueFriendAdded(IMessengerFriend messengerFriend);
    public void QueueFriendUpdated(IMessengerFriend messengerFriend);
    public void QueueFriendRemoved(int friendId);
    public List<IMessengerFriendUpdate> GetFriendListUpdates();
}
