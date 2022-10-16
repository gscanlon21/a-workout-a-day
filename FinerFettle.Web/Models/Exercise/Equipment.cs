using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Equipment used in an exercise.
    /// </summary>
    [Table("equipment"), Comment("Equipment used in an exercise")]
    [DebuggerDisplay("Name = {Name}")]
    public class Equipment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        [Required]
        public string Name { get; set; } = null!;

        public string? DisabledReason { get; set; } = null;

        [InverseProperty(nameof(EquipmentGroup.Equipment))]
        public virtual ICollection<EquipmentGroup> EquipmentGroups { get; set; } = null!;

        [InverseProperty(nameof(UserEquipment.Equipment))]
        public virtual ICollection<UserEquipment> UserEquipments { get; set; } = null!;
    }
}
