using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Web.Entities.User;

/// <summary>
/// Maps a user with their equipment.
/// </summary>
[Table("user_equipment")]
public class UserEquipment
{
    [ForeignKey(nameof(Entities.Equipment.Equipment.Id))]
    public int EquipmentId { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserEquipments))]
    public virtual User User { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Entities.Equipment.Equipment.UserEquipments))]
    public virtual Equipment.Equipment Equipment { get; private init; } = null!;
}
