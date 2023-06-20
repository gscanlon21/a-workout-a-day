using App.Dtos.Equipment;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace App.Dtos.User;

/// <summary>
/// Maps a user with their equipment.
/// </summary>
[Table("user_equipment")]
public class UserEquipment
{
    [ForeignKey(nameof(EquipmentDto.Id))]
    public int EquipmentId { get; init; }

    [ForeignKey(nameof(Dtos.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserEquipments))]
    public virtual User User { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(EquipmentDto.UserEquipments))]
    public virtual EquipmentDto Equipment { get; init; } = null!;
}
