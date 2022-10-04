using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Intensity level of an exercise variation
    /// </summary>
    [Table(nameof(IntensityPreference)), Comment("Intensity level of an exercise variation per user's strengthing preference")]
    public class IntensityPreference
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        public Proficiency2 Proficiency { get; set; } = null!;

        public StrengtheningPreference StrengtheningPreference { get; set; }

        public Intensity Intensity { get; set; } = null!; 
    }

    /// <summary>
    /// The number of sets/reps and secs that an exercise should be performed for.
    /// </summary>
    [Owned]
    public record Proficiency2(int? Secs)
    {
        public int? MinReps { get; set; }
        public int? MaxReps { get; set; }
        public int Sets { get; set; }
    }
}
