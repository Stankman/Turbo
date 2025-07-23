using Turbo.Core.Game.Messenger.Constants;
using Turbo.Core.Game.Players.Constants;
using Turbo.Core.Game.Rooms.Object.Constants;

namespace Turbo.Core.Game.Messenger.Friends;

public interface IMessengerFriendData
{
    public int Id { get; }
    public string Name { get; set; }
    public AvatarGender Gender { get; set; }
    public PlayerStatusEnum Status { get; set; }
    public string Figure { get; set; }
    public string Motto { get; set; }
}
