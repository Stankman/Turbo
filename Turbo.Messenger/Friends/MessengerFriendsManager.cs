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
    private readonly List<IMessengerFriend> _friends = [];
    private readonly Dictionary<int, MessengerFriendUpdateStateEnum> _friendUpdates = new();

    public IReadOnlyList<IMessengerFriend> Friends => _friends.AsReadOnly();

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
                var friendPlayer = await _playerManager.GetPlayerById(messengerFriendEntity.FriendPlayerId);

                if (friendPlayer == null) continue;

                var messengerFriend = _messengerFriendsFactory.CreateMessengerFriend(messengerFriendEntity, friendPlayer);
                _friends.Add(messengerFriend);
            }
        }
    }

    public IMessengerFriend? GetMessengerFriendAsync(int friendId)
    {
        return _friends.FirstOrDefault(f => f.FriendPlayerEntityId == friendId);
    }

    public async Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AddMutualFriendsAsync(IPlayer friendPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        var (playerSideEntity, friendSideEntity) = await messengerFriendsRepository.AddMutualFriendsAsync(_messenger.Id, friendPlayer.Id);

        var playerMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(playerSideEntity, friendPlayer);
        InternalAddFriend(playerMessengerFriend);

        IMessengerFriend? friendMessengerFriend = null;

        if (friendPlayer.Status.Equals(PlayerStatusEnum.Online) &&
            friendPlayer.Messenger?.MessengerFriendsManager != null)
        {
            friendMessengerFriend = _messengerFriendsFactory.CreateMessengerFriend(friendSideEntity, _messenger.Player);
            friendPlayer.Messenger.MessengerFriendsManager.InternalAddFriend(friendMessengerFriend);
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

        InternalRemoveFriend(friendPlayer.Id);

        if (friendPlayer.Status.Equals(PlayerStatusEnum.Online) &&
            friendPlayer.Messenger?.MessengerFriendsManager != null)
        {
            var friendMessengerFriend = friendPlayer.Messenger.MessengerFriendsManager.GetMessengerFriendAsync(_messenger.Id);

            if(friendMessengerFriend == null) return false;

            friendPlayer.Messenger.MessengerFriendsManager.InternalRemoveFriend(_messenger.Id);
        }

        return true;
    }

    public void InternalAddFriend(IMessengerFriend messengerFriend)
    {
        if (_friends.Any(f => f.FriendPlayerEntityId == messengerFriend.FriendPlayerEntityId))
            return;

        _friends.Add(messengerFriend);
        _friendUpdates[messengerFriend.FriendPlayerEntityId] = MessengerFriendUpdateStateEnum.Added;
    }

    public void InternalRemoveFriend(int friendId)
    {
        var messengerFriend = _friends.FirstOrDefault(f => f.FriendPlayerEntityId == friendId);
        if (messengerFriend != null)
        {
            _friends.Remove(messengerFriend);
        }

        _friendUpdates[friendId] = MessengerFriendUpdateStateEnum.Removed;
    }

    public void MarkFriendAsUpdated(int friendId)
    {
        if (_friends.Any(f => f.FriendPlayerEntityId == friendId))
        {
            _friendUpdates[friendId] = MessengerFriendUpdateStateEnum.Updated;
        }
    }

    public List<int> GetRemovedFriendIds()
    {
        return _friendUpdates
            .Where(kv => kv.Value == MessengerFriendUpdateStateEnum.Removed)
            .Select(kv => kv.Key)
            .ToList();
    }

    public List<IMessengerFriend> GetFriendsByUpdateType(MessengerFriendUpdateStateEnum state)
    {
        return _friendUpdates
            .Where(kv => kv.Value == state)
            .Select(kv => _friends.FirstOrDefault(f => f.FriendPlayerEntityId == kv.Key))
            .Where(f => f != null)
            .ToList()!;
    }

    public void ClearFriendUpdateStates()
    {
        _friendUpdates.Clear();
    }

    private async Task UnloadFriends()
    {
        _friends.Clear();
        await Task.CompletedTask;
    }

    protected override Task OnDispose() => UnloadFriends();
}
