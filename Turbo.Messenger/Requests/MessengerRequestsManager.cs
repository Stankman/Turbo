using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Game.Messenger;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;
using Turbo.Database.Repositories.Messenger;

namespace Turbo.Messenger.Requests;

public class MessengerRequestsManager(
        ILogger<IMessengerRequestsManager> _logger,
        IServiceScopeFactory _serviceScopeFactory,
        IMessengerRequestsFactory _messengerRequestsFactory,
        IMessenger _messenger
) : Component, IMessengerRequestsManager
{
    private readonly List<IMessengerRequest> _requests = [];

    public IReadOnlyList<IMessengerRequest> Requests => _requests.AsReadOnly();

    protected override Task OnInit() => LoadFriendRequests();

    private async Task LoadFriendRequests()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();
        var requestEntities = await messengerRequestsRepository.FindPlayerRequestsAsync(_messenger.Id);

        _requests.Clear();

        if (requestEntities != null)
        {
            foreach (var requestEntity in requestEntities)
            {
                var request = _messengerRequestsFactory.CreateMessengerRequest(requestEntity);
                _requests.Add(request);
            }
        }
    }

    public Task<IMessengerRequest?> GetFriendRequestAsync(int requestedByPlayerId)
    {
        var request = _requests.FirstOrDefault(r =>
            r is MessengerRequest messengerRequest &&
            messengerRequest.PlayerEntityId == requestedByPlayerId &&
            messengerRequest.TargetPlayerEntityId == _messenger.Id
        );

        return Task.FromResult(request);
    }

    public async Task<IMessengerRequest?> CreateFriendRequestAsync(IPlayer targetPlayer)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();

        var newRequestEntity = await messengerRequestsRepository.CreateRequestAsync(_messenger.Id, targetPlayer.Id);

        if (newRequestEntity != null)
        {
            var request = _messengerRequestsFactory.CreateMessengerRequest(newRequestEntity);
            
            if (targetPlayer.Session != null)
            {
                targetPlayer.Messenger.MessengerRequestsManager.InternalAddRequest(request);
            }
            
            return request;
        }

        return null;
    }

    public async Task DeleteFriendRequestAsync(int playerId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();

        await messengerRequestsRepository.DeleteRequestAsync(playerId, _messenger.Id);

        _requests.RemoveAll(request =>
            request is MessengerRequest messengerRequest &&
            messengerRequest.PlayerEntityId == playerId &&
            messengerRequest.TargetPlayerEntityId == _messenger.Id
        );
    }

    public async Task ClearFriendRequestsAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var messengerRequestsRepository = scope.ServiceProvider.GetRequiredService<IMessengerRequestsRepository>();
        
        await messengerRequestsRepository.ClearFriendRequestsAsync(_messenger.Id);

        _requests.Clear();
    }

    public void InternalAddRequest(IMessengerRequest request)
    {
        if (_requests.Any(r =>
            r is MessengerRequest mr &&
            mr.PlayerEntityId == request.PlayerEntityId))
        {
            return;
        }

        _requests.Add(request);
    }

    private async Task UnloadFriendRequests()
    {
        _requests.Clear();
        await Task.CompletedTask;
    }

    protected override Task OnDispose() => UnloadFriendRequests();
}
