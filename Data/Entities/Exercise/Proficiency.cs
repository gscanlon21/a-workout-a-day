namespace Data.Entities.Exercise;

/// <summary>
/// The number of secs/reps and sets that an exercise should be performed for.
/// </summary>
public record Proficiency(int? MinSecs, int? MaxSecs, int? MinReps, int? MaxReps)
{
    public Proficiency(int? secs, int? reps) : this(secs, secs, reps, reps) { }

    /// <summary>
    /// Set to a value to show the desired number of sets.
    /// Set to null to show the total secs/reps.
    /// </summary>
    public int? Sets { get; init; }

    /// <summary>
    /// Having to finagle this a bit. 
    /// We don't track tempo for reps, which creates an imbalance between rep and time based exercises.
    /// So I'm weighting rep-based exercises more.
    /// 
    /// ~24 per exercise: 6reps * 4sets; 8reps * 3sets; 12reps * 2sets; 60s total TUT / 2.5.
    /// </summary>
    public double Volume => MinReps.GetValueOrDefault() > 0 ? (MinReps.GetValueOrDefault() * (Sets ?? 1)) : (MinSecs.GetValueOrDefault() * (Sets ?? 1) / 2.5d);
}
