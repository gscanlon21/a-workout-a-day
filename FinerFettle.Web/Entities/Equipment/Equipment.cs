using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Equipment
{
    /// <summary>
    /// Equipment used in an exercise.
    /// </summary>
    [Table("equipment"), Comment("Equipment used in an exercise")]
    [DebuggerDisplay("Name = {Name}")]
    public class Equipment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private init; }

        [Required]
        public string Name { get; private init; } = null!;

        public string? DisabledReason { get; private init; } = null;

        [InverseProperty(nameof(EquipmentGroup.Equipment))]
        public virtual ICollection<EquipmentGroup> EquipmentGroups { get; private init; } = null!;

        [InverseProperty(nameof(UserEquipment.Equipment))]
        public virtual ICollection<UserEquipment> UserEquipments { get; private init; } = null!;
    }
}
