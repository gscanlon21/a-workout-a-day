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
    [Table("intensity"), Comment("Intensity level of an exercise variation per user's strengthing preference")]
    public class Intensity
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; init; }

        public string? DisabledReason { get; set; } = null;

        public Proficiency Proficiency { get; set; } = null!;

        public Variation Variation { get; set; } = null!;

        public IntensityLevel IntensityLevel { get; set; }
    }

    /// <summary>
    /// The number of sets/reps and secs that an exercise should be performed for.
    /// </summary>
    [Owned]
    public record Proficiency(int? Secs)
    {
        public int? MinReps { get; set; }
        public int? MaxReps { get; set; }
        public int Sets { get; set; }
    }

    /// <summary>
    /// Maintain/Obtain/Gain/Endurance/Recovery/WarmupCooldown
    /// </summary>
    public enum IntensityLevel
    {
        Maintain = 0,
        Obtain = 1,
        Gain = 2,
        Endurance = 3,
        Recovery = 4,
        WarmupCooldown = 5 // Might split these out at some point 
    }
}
