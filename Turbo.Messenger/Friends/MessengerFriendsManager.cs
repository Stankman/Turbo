using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;
using Turbo.Database.Repositories.Messenger;

namespace Turbo.Messenger.Friends;

public class MessengerFriendsManager(
    ILogger<IMessengerRequestsManager> _logger,
    IServiceScopeFactory _serviceScopeFactory,
    IMessenger _messenger
) : Component, IMessengerFriendsManager
{
    private readonly List<IMessengerFriend> _friends = [];
    public IReadOnlyList<IMessengerFriend> Friends => _friends.AsReadOnly();

    protected override Task OnInit() => LoadFriends();

    private async Task LoadFriends()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();
        var friendEntities = await messengerFriendsRepository.FindAllByPlayerIdAsync(_messenger.Id);

        _friends.Clear();
        if (friendEntities != null)
        {
            foreach (var friendEntity in friendEntities)
            {
                var friend = new MessengerFriend(friendEntity);
                _friends.Add(friend);
            }
        }
    }

    public async Task AddFriendAsync(IPlayer targetPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        var newFriendEntity = await messengerFriendsRepository.AddFriendAsync(_messenger.Id, targetPlayer.Id);

        if (newFriendEntity != null)
        {
            var friend = new MessengerFriend(newFriendEntity);
            _friends.Add(friend);
        }
    }

    public async Task RemoveFriendAsync(IPlayer targetPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerFriendsRepository = scope.ServiceProvider.GetRequiredService<IMessengerFriendsRepository>();

        await messengerFriendsRepository.RemoveFriendAsync(_messenger.Id, targetPlayer.Id);

        _friends.RemoveAll(friend =>
            friend is MessengerFriend messengerFriend && messengerFriend.FriendPlayerEntityId == targetPlayer.Id
        );
    }

    private async Task UnloadFriends()
    {
        _friends.Clear();
        await Task.CompletedTask;
    }

    protected override Task OnDispose() => UnloadFriends();
}
