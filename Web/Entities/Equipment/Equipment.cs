using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;
using Web.Entities.User;

namespace Web.Entities.Equipment;

/// <summary>
/// Equipment used in an exercise.
/// </summary>
[Table("equipment"), Comment("Equipment used in an exercise")]
[DebuggerDisplay("Name = {Name}")]
public class Equipment
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; private init; } = null!;

    public string? DisabledReason { get; private init; } = null;

    [JsonIgnore, InverseProperty(nameof(Instruction.Equipment))]
    public virtual ICollection<Instruction> Instructions { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserEquipment.Equipment))]
    public virtual ICollection<UserEquipment> UserEquipments { get; private init; } = null!;
}
