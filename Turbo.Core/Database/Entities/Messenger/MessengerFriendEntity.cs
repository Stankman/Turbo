using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Turbo.Core.Database.Attributes;
using Turbo.Core.Database.Entities.Players;
using Turbo.Core.Game.Messenger.Constants;

namespace Turbo.Core.Database.Entities.Messenger;

[Table("messenger_friends")]
[Index(nameof(PlayerId), nameof(FriendPlayerId), IsUnique = true)]
public class MessengerFriendEntity : Entity
{
    [Column("player_id")][Required] public int PlayerId { get; set; }

    [Column("requested_id")][Required] public int FriendPlayerId { get; set; }

    [Column("category_id")] public int? MessengerCategoryEntityId { get; set; }

    [Column("relation")]
    [Required]
    [DefaultValueSql(MessengerFriendRelationEnum.Zero)]
    public MessengerFriendRelationEnum RelationType { get; set; }

    [ForeignKey(nameof(PlayerId))] public PlayerEntity 
    PlayerEntity { get; set; }

    [ForeignKey(nameof(FriendPlayerId))]
    public PlayerEntity FriendPlayerEntity { get; set; }

    [ForeignKey(nameof(MessengerCategoryEntityId))]
    public MessengerCategoryEntity MessengerCategoryEntity { get; set; }
}