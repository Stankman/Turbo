using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Messenger.Requests;
using Turbo.Core.Game.Players;
using Turbo.Core.Utilities;

namespace Turbo.Core.Game.Messenger;

public interface IMessenger : IComponent
{
    public ILogger<IMessenger> Logger { get; }
    public IMessengerManager MessengerManager { get; }
    public int Id { get; }
    public IMessengerFriendManager MessengerFriendsManager { get; }
    public IMessengerRequestsManager MessengerRequestsManager { get; }
    public event EventHandler? AddedNewFriendsEvent;
    public event EventHandler<List<int>>? RemovedFriendsEvent;
    public List<IMessengerRequest> PendingRequests { get; }
    public List<IMessengerFriend> Friends { get; }
    public bool HasSentRequestTo(int targetPlayerId);
    public bool HasPendingFriendRequestFrom(int senderPlayerId);
    public Task<IMessengerRequest?> SendFriendRequest(IPlayer targetPlayer);
    public Task AcceptFriendRequests(List<int> playerIds);
    public Task DeclineFriendRequests(List<int> playerId);
    public Task DeclineAll();
    public Task DeleteFriends(List<int> friendIds);
    public bool IsFriendWith(int playerId);
    public IMessengerFriend? GetFriendById(int friendPlayerId);
}
