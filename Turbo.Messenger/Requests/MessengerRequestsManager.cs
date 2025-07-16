using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;
using Turbo.Database.Repositories.Messenger;

namespace Turbo.Messenger.Requests;

public class MessengerRequestsManager(
        ILogger<IMessengerRequestsManager> _logger,
        IServiceScopeFactory _serviceScopeFactory,
        IMessenger _messenger
) : Component, IMessengerRequestsManager
{
    private readonly List<IMessengerRequest> _requests = [];

    public IReadOnlyList<IMessengerRequest> Requests => _requests.AsReadOnly();

    protected override Task OnInit() => LoadFriendRequests();

    public async Task LoadFriendRequests()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();
        var requestEntities = await messengerRequestsRepository.FindAllByPlayerIdAsync(_messenger.Id);

        _requests.Clear();
        if (requestEntities != null)
        {
            foreach (var requestEntity in requestEntities)
            {
                var request = new MessengerRequest(requestEntity);
                _requests.Add(request);
            }
        }
    }

    public async Task CreateFriendRequestAsync(IPlayer targetPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();

        var newRequestEntity = await messengerRequestsRepository.CreateRequestAsync(_messenger.Id, targetPlayer.Id);

        if (newRequestEntity != null)
        {
            var request = new MessengerRequest(newRequestEntity);
            _requests.Add(request);
        }
    }

    public async Task ClearFriendRequestsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();
        await messengerRequestsRepository.ClearFriendRequestsAsync(_messenger.Id);
        _requests.Clear();
    }

    public async Task DeleteFriendRequestAsync(IPlayer requestedByPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();

        await messengerRequestsRepository.DeleteRequestAsync(requestedByPlayer.Id, _messenger.Id);

        _requests.RemoveAll(request =>
            request is MessengerRequest messengerRequest &&
            messengerRequest.PlayerEntityId == requestedByPlayer.Id &&
            messengerRequest.RequestedPlayerEntityId == _messenger.Id
        );
    }

    private async Task UnloadFriendRequests()
    {
        _requests.Clear();
        await Task.CompletedTask;
    }

    protected override Task OnDispose() => UnloadFriendRequests();
}
