using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.Models.Exercise
{
    [Table(nameof(Intensity)), Comment("Intensity level of an exercise variation")]
    public class Intensity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Proficiency Proficiency { get; set; } = null!;

        [Required]
        public IntensityLevel IntensityLevel { get; set; }

        [Range(0, 100)]
        public int? MinProgression { get; set; }

        [Range(0, 100)]
        public int? MaxProgression { get; set; }

        [InverseProperty(nameof(Models.Exercise.Variation.Intensities))]
        public virtual Variation Variation { get; set; } = null!;
    }

    public enum IntensityLevel
    {
        Main = 0,
        Stretch = 1,
    }

    [Owned]
    public record Proficiency(int? Reps, int? Secs)
    {
        public int Sets { get; set; }
    }
}
