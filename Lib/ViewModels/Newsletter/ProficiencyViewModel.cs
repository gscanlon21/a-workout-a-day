namespace Lib.ViewModels.Newsletter;

/// <summary>
/// The number of sets/reps and secs that an exercise should be performed for.
/// </summary>
public class ProficiencyViewModel
{
    /// <summary>
    /// Set to a value to show the desired number of sets.
    /// Set to null to show the total secs/reps.
    /// </summary>
    public int? Sets { get; set; }

    public int? MinReps { get; set; }

    public int? MaxSecs { get; set; }

    public int? MinSecs { get; set; }

    public int? MaxReps { get; set; }

    public bool HasReps => MinReps.HasValue || MaxReps.HasValue;

    public bool HasSecs => MinSecs.HasValue || MaxSecs.HasValue;
}
