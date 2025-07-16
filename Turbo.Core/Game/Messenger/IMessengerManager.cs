using System.Threading.Tasks;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger;

public interface IMessengerManager : IComponent
{
    public IMessenger? GetMessengerForPlayer(IPlayer player);
    public Task<IMessenger> AddMessenger(IMessenger messenger);
    public Task RemoveMessenger(int playerId);
}
