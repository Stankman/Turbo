using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Utilities;
using Turbo.Database.Repositories.Messenger;
using Turbo.Packets.Outgoing.FriendList;

namespace Turbo.Messenger.Friends;

public class MessengerFriendManager(
    ILogger<IMessengerFriendManager> _logger,
    IServiceScopeFactory _serviceScopeFactory,
    IMessengerFriendsFactory _messengerFriendsFactory,
    IPlayerManager _playerManager,
    IMessenger _messenger
) : Component, IMessengerFriendManager
{
    private readonly ConcurrentDictionary<int, IMessengerFriend> _friends = new();
    private readonly ConcurrentQueue<IMessengerFriendUpdate> _friendUpdates = new();

    public IReadOnlyCollection<IMessengerFriend> Friends => _friends.Values.ToList();

    protected override Task OnInit() => LoadFriends();

    private async Task LoadFriends()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();
        var messengerFriendEntities = await messengerFriendsRepository.FindAllByPlayerIdAsync(_messenger.Id);

        _friends.Clear();
        if (messengerFriendEntities != null)
        {
            foreach (var messengerFriendEntity in messengerFriendEntities)
            {
                var friendPlayer = await _playerManager.GetOfflinePlayerById(messengerFriendEntity.FriendPlayerId);

                if (friendPlayer == null) continue;

                var messengerFriend = _messengerFriendsFactory.CreateMessengerFriend(messengerFriendEntity);
                _friends[messengerFriend.Id] = messengerFriend;
            }
        }
    }

    public IMessengerFriend? GetFriendById(int friendId)
    {
        _friends.TryGetValue(friendId, out var messengerFriend);
        return messengerFriend;
    }

    public List<List<IMessengerFriend>> GetFriendsFragments(int fragmentSize)
    {
        var friendsList = Friends.ToList();
        var totalFriends = friendsList.Count;
        var fragments = new List<List<IMessengerFriend>>();

        if (totalFriends == 0)
        {
            fragments.Add(new List<IMessengerFriend>());
            return fragments;
        }

        for (int i = 0; i < totalFriends; i += fragmentSize)
        {
            var fragment = friendsList.Skip(i).Take(fragmentSize).ToList();
            fragments.Add(fragment);
        }

        return fragments;
    }

    public async Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AddMutualFriendsAsync(IPlayer friendPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        var (playerSideEntity, friendSideEntity) = await messengerFriendsRepository.AddMutualFriendsAsync(_messenger.Id, friendPlayer.Id);

        var playerMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(playerSideEntity);

        QueueFriendAdded(playerMessengerFriend);

        IMessengerFriend? friendMessengerFriend = null;

        if (friendPlayer.Status.Equals(PlayerStatusEnum.Online) &&
            friendPlayer.Messenger?.MessengerFriendsManager != null)
        {
            friendMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(friendSideEntity);
            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendAdded(friendMessengerFriend);
        }

        return (playerMessengerFriend, friendMessengerFriend);
    }

    public async Task<bool> DeleteFriendAsync(int friendId)
    {
        var friend = GetFriendById(friendId);

        if (friend == null) return false;

        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        await messengerFriendsRepository.DeleteMutualFriendsAsync(_messenger.Id, friendId);

        QueueFriendRemoved(friendId);

        var friendPlayer = _playerManager.GetPlayerById(friendId);

        if (friendPlayer != null && friendPlayer.Status.Equals(PlayerStatusEnum.Online))
        {
            var me = friendPlayer.Messenger.MessengerFriendsManager.GetFriendById(_messenger.Id);

            if(me == null) return true;

            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendRemoved(_messenger.Id);
        }

        return true;
    }

    public void QueueFriendAdded(IMessengerFriend messengerFriend)
    {
        if (_friends.TryAdd(messengerFriend.Id, messengerFriend))
        {
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = messengerFriend.Id,
                FriendData = messengerFriend.Friend,
                UpdateType = FriendListUpdateActionEnum.Added
            });
        }
    }

    public void QueueFriendUpdated(IMessengerFriend messengerFriend)
    {
        if (_friends.ContainsKey(messengerFriend.Id))
        {
            _friends[messengerFriend.Id] = messengerFriend;
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = messengerFriend.Id,
                FriendData = messengerFriend.Friend,
                UpdateType = FriendListUpdateActionEnum.Updated
            });
        }
    }

    public void QueueFriendRemoved(int friendId)
    {
        if (_friends.ContainsKey(friendId))
        {
            _friends.TryRemove(friendId, out _);
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = friendId,
                UpdateType = FriendListUpdateActionEnum.Removed
            });
        }
    }

    public List<IMessengerFriendUpdate> GetFriendListUpdates()
    {
        var updates = new List<IMessengerFriendUpdate>();
        while (_friendUpdates.TryDequeue(out var update))
            updates.Add(update);
        return updates;
    }

    private async Task UnloadFriends()
    {
        _friends.Clear();
        await Task.CompletedTask;
    }

    protected override Task OnDispose() => UnloadFriends();
}
