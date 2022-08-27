using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Table(nameof(Equipment)), Comment("Equipment used in an exercise")]
    public class Equipment
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [InverseProperty(nameof(EquipmentGroup.Equipment))]
        public virtual ICollection<EquipmentGroup> EquipmentGroups { get; set; } = null!;

        [InverseProperty(nameof(EquipmentUser.Equipment))]
        public virtual ICollection<EquipmentUser> EquipmentUsers { get; set; } = null!;
    }

    [Table(nameof(EquipmentGroup)), Comment("Equipment that can be switched out for one another")]
    public class EquipmentGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Equipment.EquipmentGroups))]
        public List<Equipment> Equipment { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Intensity.EquipmentGroups))]
        public Intensity Intensity { get; set; } = null!;

        public bool Required { get; set; }

        public string? Instruction { get; set; }
    }
}
