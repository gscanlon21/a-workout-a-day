using Core.Models.Exercise;

namespace Core.Dtos.Newsletter;

/// <summary>
/// A day of a user's workout split.
/// </summary>
public class WorkoutRotationDto
{
    public int Id { get; init; }

    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroupsDisplayName}) {MovementPatterns.GetDisplayName(DisplayType.ShortName)}";
    }

    public MovementPattern MovementPatterns { get; init; }

    public IList<MusculoskeletalSystem> MuscleGroups { get; init; } = null!;

    public IList<MusculoskeletalSystem> CoreMuscleGroups => MuscleGroups.Intersect(MuscleGroupExtensions.Core()).ToList();

    public IList<MusculoskeletalSystem> MuscleGroupsWithCore => MuscleGroups.Union(MuscleGroupExtensions.Core()).ToList();

    public IList<MusculoskeletalSystem> MuscleGroupsSansCore => MuscleGroups.Except(MuscleGroupExtensions.Core()).ToList();

    public string MuscleGroupsDisplayName => MuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr, n) => curr | n).GetDisplayName2(DisplayType.ShortName);
}
