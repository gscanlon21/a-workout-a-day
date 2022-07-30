using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    [Table(nameof(Variation)), Comment("Progressions of an exercise")]
    [DebuggerDisplay("Code = {Code}")]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public bool Enabled { get; set; }

        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Instruction { get; set; } = null!;

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        [InverseProperty(nameof(EquipmentGroup.Variations))]
        public ICollection<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();

        [InverseProperty(nameof(Intensity.Variation))]
        public ICollection<Intensity> Intensities { get; set; } = null!;

        [InverseProperty(nameof(Models.Exercise.Exercise.Variations))]
        public virtual Exercise Exercise { get; set; } = null!;
    }
}
