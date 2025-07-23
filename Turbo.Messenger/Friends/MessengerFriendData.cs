using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Messenger.Friends;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Game.Rooms.Object.Constants;

namespace Turbo.Messenger.Friends;

public class MessengerFriendData(
    PlayerEntity _playerEntity
    ) : IMessengerFriendData
{
    public int Id { get; set; } = _playerEntity.Id;
    public string Name { get; set; } = _playerEntity.Name;
    public AvatarGender Gender { get; set; } = _playerEntity.Gender;
    public PlayerStatusEnum Status { get; set; } = _playerEntity.PlayerStatus;
    public string Figure { get; set; } = _playerEntity.Figure;
    public string Motto { get; set; } = _playerEntity.Motto ?? string.Empty;
}
