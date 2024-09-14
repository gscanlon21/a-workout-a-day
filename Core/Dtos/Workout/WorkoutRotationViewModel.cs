using Core.Models.Exercise;

namespace Core.Dtos.Workout;

/// <summary>
/// A day of a user's workout split.
/// </summary>
public record WorkoutRotationViewModel(int Id)
{
    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroupsDisplayName}) {MovementPatterns.GetDisplayName32(DisplayType.ShortName)}";
    }

    public string MuscleGroupsDisplayName => MuscleGroups.Aggregate(Core.Models.Exercise.MusculoskeletalSystem.None, (curr, n) => curr | n).GetDisplayName322(DisplayType.ShortName);

    public IList<MusculoskeletalSystem> MuscleGroups { get; set; } = null!;

    public MovementPattern MovementPatterns { get; set; }

    public IList<MusculoskeletalSystem> CoreMuscleGroups => MuscleGroups.Intersect(MuscleGroupExtensions.Core()).ToList();

    public IList<MusculoskeletalSystem> MuscleGroupsWithCore => MuscleGroups.Union(MuscleGroupExtensions.Core()).ToList();

    public IList<MusculoskeletalSystem> MuscleGroupsSansCore => MuscleGroups.Except(MuscleGroupExtensions.Core()).ToList();
}
