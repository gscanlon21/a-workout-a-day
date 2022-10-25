using FinerFettle.Web.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Entities.Equipment
{
    /// <summary>
    /// Equipment that can be switched out for one another.
    /// </summary>
    [Table("equipment_group"), Comment("Equipment that can be switched out for one another")]
    [DebuggerDisplay("Name = {Name}")]
    public class EquipmentGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; private init; }

        [Required]
        public string Name { get; private init; } = null!;

        /// <summary>
        /// Whether this set of equipment is required to do the exercise.
        /// </summary>
        [Required]
        public bool Required { get; private init; }

        /// <summary>
        /// Whether the equipment in the equipment group is used as weight/resistence for a harder workout.
        /// </summary>
        [Required]
        public bool IsWeight { get; private init; }

        /// <summary>
        /// A link to show the user how to complete the exercise w/ this equipment.
        /// </summary>
        public string? Instruction { get; private init; }

        public string? DisabledReason { get; private init; } = null;

        [InverseProperty(nameof(Entities.Equipment.Equipment.EquipmentGroups))]
        public virtual ICollection<Equipment> Equipment { get; private init; } = null!;

        [InverseProperty(nameof(Exercise.Variation.EquipmentGroups))]
        public virtual Variation Variation { get; private init; } = null!;
    }
}
