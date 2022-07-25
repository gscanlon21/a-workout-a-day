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

        [Required]
        public IList<EquipmentGroup> EquipmentGroups { get; set; } = null!;

        [Required]
        public IList<EquipmentUser> EquipmentUsers { get; set; } = null!;
    }

    [Table(nameof(EquipmentGroup)), Comment("Equipment that can be switched out for one another")]
    public class EquipmentGroup
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public IList<Equipment> Equipment { get; set; } = null!;

        [Required]
        public IList<Variation> Variations { get; set; } = null!;
    }
}
