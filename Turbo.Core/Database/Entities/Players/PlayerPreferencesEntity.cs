using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Turbo.Core.Database.Entities.Players;

[Table("player_preferences")]
[Index(nameof(PlayerId), IsUnique = true)]
public class PlayerPreferencesEntity : Entity
{
    [Column("player_id")]
    [Required]
    public int PlayerId { get; set; }

    [Column("block_friend_requests")]
    [Required]
    public bool BlockFriendRequests { get; set; }

    [Column("allow_friend_follow")]
    [Required]
    public bool AllowFriendFollow { get; set; }

    [ForeignKey(nameof(PlayerId))]
    public PlayerEntity PlayerEntity { get; set; }
}