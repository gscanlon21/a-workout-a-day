using Core.Code.Extensions;
using Core.Models.Exercise;

namespace Core.Dtos.Workout;

/// <summary>
/// A day of a user's workout split.
/// </summary>
public record WorkoutRotationViewModel(int Id)
{
    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroupsDisplayName}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayType.ShortName)}";
    }

    public string MuscleGroupsDisplayName => MuscleGroups.Aggregate(Core.Models.Exercise.MuscleGroups.None, (curr, n) => curr | n).GetDisplayName322(EnumExtensions.DisplayType.ShortName);

    public IList<MuscleGroups> MuscleGroups { get; set; } = null!;

    public MovementPattern MovementPatterns { get; set; }

    public IList<MuscleGroups> CoreMuscleGroups => MuscleGroups.Intersect(MuscleGroupExtensions.Core()).ToList();

    public IList<MuscleGroups> MuscleGroupsWithCore => MuscleGroups.Union(MuscleGroupExtensions.Core()).ToList();

    public IList<MuscleGroups> MuscleGroupsSansCore => MuscleGroups.Except(MuscleGroupExtensions.Core()).ToList();
}
