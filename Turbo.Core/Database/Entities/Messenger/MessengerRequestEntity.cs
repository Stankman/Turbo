using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Turbo.Core.Database.Entities.Players;

namespace Turbo.Core.Database.Entities.Messenger;

[Table("messenger_requests")]
[Index(nameof(PlayerId), nameof(RequestedPlayerId), IsUnique = true)]
public class MessengerRequestEntity : Entity
{
    [Column("player_id")][Required] public int PlayerId { get; set; }

    [Column("requested_id")][Required] public int RequestedPlayerId { get; set; }

    [ForeignKey(nameof(PlayerId))] 
    public PlayerEntity PlayerEntity { get; set; }

    [ForeignKey(nameof(RequestedPlayerId))]
    public PlayerEntity RequestedPlayerEntity { get; set; }
}