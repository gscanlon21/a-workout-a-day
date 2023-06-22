using Lib.Dtos.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace Lib.Dtos.Equipment;

/// <summary>
/// Equipment used in an exercise.
/// </summary>
[Table("equipment")]
[DebuggerDisplay("Name = {Name}")]
public class EquipmentDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; init; } = null!;

    public string? DisabledReason { get; init; } = null;

    //[JsonIgnore, InverseProperty(nameof(Instruction.Equipment))]
    public virtual ICollection<Instruction> Instructions { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserEquipment.Equipment))]
    public virtual ICollection<UserEquipment> UserEquipments { get; init; } = null!;
}
