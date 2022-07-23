using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Comment("Progressions of an exercise"), Table(nameof(Variation))]
    public class Variation
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public string Code { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Instruction { get; set; } = null!;

        [Range(0, 100)]
        public int? Progression { get; set; }

        [Required]
        public Equipment Equipment { get; set; }

        [Required]
        public MuscleContractions MuscleContractions { get; set; }

        [Required]
        public IList<Intensity> Intensities { get; set; } = null!;
    }
}
