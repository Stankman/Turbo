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

namespace Turbo.Messenger.Friends;

public class MessengerFriendsManager(
    ILogger<IMessengerRequestsManager> _logger,
    IServiceScopeFactory _serviceScopeFactory,
    IMessengerFriendsFactory _messengerFriendsFactory,
    IPlayerManager _playerManager,
    IMessenger _messenger
) : Component, IMessengerFriendsManager
{
    private readonly ConcurrentDictionary<int, IMessengerFriend> _friends = new();
    private readonly ConcurrentQueue<IMessengerFriendUpdate> _friendUpdates = new();

    public IReadOnlyCollection<IMessengerFriend> Friends => _friends.Values;
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
                var friendPlayer = _playerManager.GetPlayerById(messengerFriendEntity.FriendPlayerId);

                if (friendPlayer == null) continue;

                var messengerFriend = _messengerFriendsFactory.CreateMessengerFriend(messengerFriendEntity, friendPlayer);
                _friends[messengerFriend.FriendPlayerEntityId] = messengerFriend;
            }
        }
    }

    public IMessengerFriend? GetMessengerFriendAsync(int friendId)
    {
        _friends.TryGetValue(friendId, out var messengerFriend);
        return messengerFriend;
    }

    public async Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AddMutualFriendsAsync(IPlayer friendPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        var (playerSideEntity, friendSideEntity) = await messengerFriendsRepository.AddMutualFriendsAsync(_messenger.Id, friendPlayer.Id);

        var playerMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(playerSideEntity, friendPlayer);
        QueueFriendAdded(playerMessengerFriend);

        IMessengerFriend? friendMessengerFriend = null;

        if (friendPlayer.Status.Equals(PlayerStatusEnum.Online) &&
            friendPlayer.Messenger?.MessengerFriendsManager != null)
        {
            friendMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(friendSideEntity, _messenger.Player);
            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendAdded(friendMessengerFriend);
        }

        return (playerMessengerFriend, friendMessengerFriend);
    }

    public async Task<bool> DeleteFriendAsync(IPlayer friendPlayer)
    {
        var playerMessengerFriend = GetMessengerFriendAsync(friendPlayer.Id);

        if (playerMessengerFriend == null) return false;

        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        await messengerFriendsRepository.DeleteMutualFriendsAsync(_messenger.Id, friendPlayer.Id);

        QueueFriendRemoved(friendPlayer.Id);

        if (friendPlayer.Status.Equals(PlayerStatusEnum.Online) &&
            friendPlayer.Messenger?.MessengerFriendsManager != null)
        {
            var friendMessengerFriend = friendPlayer.Messenger.MessengerFriendsManager.GetMessengerFriendAsync(_messenger.Id);

            if(friendMessengerFriend == null) return false;

            friendPlayer.Messenger.MessengerFriendsManager.QueueFriendRemoved(_messenger.Id);
        }

        return true;
    }

    public void QueueFriendAdded(IMessengerFriend messengerFriend)
    {
        if (_friends.TryAdd(messengerFriend.FriendPlayerEntityId, messengerFriend))
        {
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = messengerFriend.FriendPlayerEntityId,
                FriendData = messengerFriend,
                UpdateType = FriendListUpdateActionEnum.Added
            });
        }
    }

    public void QueueFriendUpdated(IMessengerFriend messengerFriend)
    {
        if (_friends.ContainsKey(messengerFriend.FriendPlayerEntityId))
        {
            _friends[messengerFriend.FriendPlayerEntityId] = messengerFriend;
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = messengerFriend.FriendPlayerEntityId,
                FriendData = messengerFriend,
                UpdateType = FriendListUpdateActionEnum.Updated
            });
        }
    }

    public void QueueFriendRemoved(int friendId)
    {
        if(_friends.ContainsKey(friendId))
        {
            _friends.TryRemove(friendId, out _);
            _friendUpdates.Enqueue(new MessengerFriendUpdate
            {
                FriendId = friendId,
                UpdateType = FriendListUpdateActionEnum.Removed
            });
        }
    }

    public List<IMessengerFriendUpdate> DrainFriendUpdates()
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
