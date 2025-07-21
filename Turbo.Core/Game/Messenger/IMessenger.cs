using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Packets.Messages;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger;

public interface IMessenger : IComponent
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public int Id { get; }
    public IPlayer Player { get; }
    public IMessengerFriendsManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }
    public bool HasSentRequestTo(int targetPlayerId);
    public bool HasReceivedRequestFrom(int senderPlayerId);
    public List<IMessengerRequest> GetPendingRequests();
    public Task<IMessengerRequest?> SendFriendRequest(IPlayer targetPlayer);
    public Task<(IMessengerFriend? playerMessengerFriend, IMessengerFriend? friendMessengerFriend)> AcceptFriend(int playerId);
    public Task DeclineFriend(int playerId);
    public Task DeclineAll();
    public bool IsFriendWith(int playerId);
    public bool HasPendingFriendRequestFrom(int playerId);
    public Task SendComposer(IComposer composer);
}
