using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Table(nameof(Variation)), Comment("Progressions of an exercise")]
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

        [Required]
        public IList<EquipmentGroup> EquipmentGroups { get; set; } = new List<EquipmentGroup>();

        [Required]
        public IList<Intensity> Intensities { get; set; } = null!;
    }
}
