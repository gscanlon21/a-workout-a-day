using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Exercise;

namespace Web.Entities.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
[Table("intensity"), Comment("Intensity level of an exercise variation per user's strengthing preference")]
public class Intensity
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    public string? DisabledReason { get; private init; } = null;

    public Proficiency Proficiency { get; private init; } = null!;

    public Variation Variation { get; private init; } = null!;

    public IntensityLevel IntensityLevel { get; private init; }
}

/// <summary>
/// The number of sets/reps and secs that an exercise should be performed for.
/// </summary>
[Owned]
public record Proficiency(int? MinSecs, int? MaxSecs, int? MinReps, int? MaxReps)
{
    /// <summary>
    /// Set to a value to show the desired number of sets.
    /// Set to null to show the total secs/reps.
    /// </summary>
    public int? Sets { get; set; }

    private bool HasReps => MinReps != null || MaxReps != null;

    private double AvgReps => ((MinReps ?? 0) + (MaxReps ?? 0)) / 2d;

    private double AvgSecs => ((MinSecs ?? 0) + (MaxSecs ?? 0)) / 2d;

    /// <summary>
    /// Having to finagle this a bit. 
    /// We don't track tempo for reps, which creates an imbalance between rep and time based exercises.
    /// So I'm weighting rep-based exercises quadrupled.
    /// </summary>
    public double TimeUnderTension => HasReps ? (AvgReps * (Sets ?? 1) * 2d) : (AvgSecs * (Sets ?? 1) / 2d);
}
