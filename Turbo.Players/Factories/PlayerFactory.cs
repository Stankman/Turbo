using System;
using Microsoft.Extensions.DependencyInjection;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Database.Factories.Messenger;
using Turbo.Core.Database.Factories.Players;
using Turbo.Core.Game.Players;

namespace Turbo.Players.Factories;

public class PlayerFactory(
    IPlayerInventoryFactory _playerInventoryFactory,
    IMessengerFactory _messengerFactory,
    IServiceScopeFactory _serviceScopeFactory,
    IServiceProvider _provider) : IPlayerFactory
{
    public IPlayer Create(PlayerEntity playerEntity, PlayerPreferencesEntity playerPreferencesEntity)
    {
        var playerDetails = ActivatorUtilities.CreateInstance<PlayerDetails>(_provider, playerEntity);
        var playerPreferences = ActivatorUtilities.CreateInstance<PlayerPreferences>(_provider, playerPreferencesEntity);
        var player = ActivatorUtilities.CreateInstance<Player>(_provider, playerDetails, playerPreferences);
        var inventory = _playerInventoryFactory.Create(player);
        var wallet = new PlayerWallet(player, _serviceScopeFactory);
        var perks = new PlayerPerks(player, _serviceScopeFactory);
        var messenger = _messengerFactory.Create(player);

        player.SetInventory(inventory);
        player.SetWallet(wallet);
        player.SetPerks(perks);
        player.SetMessenger(messenger);

        return player;
    }
}