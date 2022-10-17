using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Equipment that can be switched out for one another.
    /// </summary>
    [Table("equipment_group"), Comment("Equipment that can be switched out for one another")]
    [DebuggerDisplay("Name = {Name}")]
    public class EquipmentGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public string Name { get; set; } = null!;

        public string? DisabledReason { get; set; } = null;

        [InverseProperty(nameof(Models.Exercise.Equipment.EquipmentGroups))]
        public List<Equipment> Equipment { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Variation.EquipmentGroups))]
        public Variation Variation { get; set; } = null!;

        /// <summary>
        /// Whether this set of equipment is required to do the exercise.
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// A link to show the user how to complete the exercise w/ this equipment.
        /// </summary>
        public string? Instruction { get; set; }

        /// <summary>
        /// Whether the equipment in the equipment group is used as weight/resistence for a harder workout.
        /// </summary>
        public bool IsWeight { get; set; }
    }
}
