using Core.Models.Exercise;


namespace Lib.ViewModels.Exercise;

/// <summary>
/// Intensity level of an exercise variation
/// </summary>
public class IntensityViewModel
{
    public int Id { get; init; }

    public string? DisabledReason { get; init; } = null;

    public Proficiency Proficiency { get; init; } = null!;

    //public Variation Variation { get; init; } = null!;

    public IntensityLevel IntensityLevel { get; init; }
}

/// <summary>
/// The number of sets/reps and secs that an exercise should be performed for.
/// </summary>
public record Proficiency(int? MinSecs, int? MaxSecs, int? MinReps, int? MaxReps)
{
    /// <summary>
    /// Set to a value to show the desired number of sets.
    /// Set to null to show the total secs/reps.
    /// </summary>
    public int? Sets { get; set; }

    //private bool HasReps => MinReps != null || MaxReps != null;

    //private bool HasSecs => MinSecs != null || MaxSecs != null;
}
