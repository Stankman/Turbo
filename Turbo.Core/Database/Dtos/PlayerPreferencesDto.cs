namespace Turbo.Core.Database.Dtos;

public class PlayerPreferencesDto
{
    public int PlayerEntityId { get; set; }
    public bool BlockFriendRequests { get; set; }
    public bool AllowFriendsFollow { get; set; }
}
