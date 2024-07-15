namespace Core.Dtos.Exercise;

/// <summary>
/// The number of secs/reps and sets that an exercise should be performed for.
/// </summary>
public record ProficiencyDto(int? MinSecs, int? MaxSecs, int? MinReps, int? MaxReps)
{
    /// <summary>
    /// Set to a value to show the desired number of sets.
    /// Set to null to show the total secs/reps.
    /// </summary>
    public int? Sets { get; init; }

    public bool HasReps => MinReps.HasValue || MaxReps.HasValue;
    public bool HasSecs => MinSecs.HasValue || MaxSecs.HasValue;
}
