using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Dtos.Newsletter;

/// <summary>
/// A day of a user's workout split.
/// </summary>
public class WorkoutRotationDto
{
    public int Id { get; init; }

    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroupsDisplayName}) {MovementPatterns.GetDisplayName32(DisplayType.ShortName)}";
    }

    [NotMapped]
    public string MuscleGroupsDisplayName => MuscleGroups.Aggregate(Models.Exercise.MusculoskeletalSystem.None, (curr, n) => curr | n).GetDisplayName322(DisplayType.ShortName);

    /// <summary>
    /// May or may not contain the core muscles, depends on the user's workout split preferences.
    /// </summary>
    public IList<MusculoskeletalSystem> MuscleGroups { get; set; } = null!;

    public MovementPattern MovementPatterns { get; set; }

    [NotMapped]
    public IList<MusculoskeletalSystem> CoreMuscleGroups => MuscleGroups.Intersect(MuscleGroupExtensions.Core()).ToList();

    [NotMapped]
    public IList<MusculoskeletalSystem> MuscleGroupsWithCore => MuscleGroups.Union(MuscleGroupExtensions.Core()).ToList();

    [NotMapped]
    public IList<MusculoskeletalSystem> MuscleGroupsSansCore => MuscleGroups.Except(MuscleGroupExtensions.Core()).ToList();
}
