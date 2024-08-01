using Core.Code.Extensions;
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
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroupsDisplayName}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayType.ShortName)}";
    }

    [NotMapped]
    public string MuscleGroupsDisplayName => MuscleGroups.Aggregate(Models.Exercise.MuscleGroups.None, (curr, n) => curr | n).GetDisplayName322(EnumExtensions.DisplayType.ShortName);

    /// <summary>
    /// May or may not contain the core muscles, depends on the user's workout split preferences.
    /// </summary>
    public IList<MuscleGroups> MuscleGroups { get; set; } = null!;

    public MovementPattern MovementPatterns { get; set; }

    [NotMapped]
    public IList<MuscleGroups> CoreMuscleGroups => MuscleGroups.Intersect(MuscleGroupExtensions.Core()).ToList();

    [NotMapped]
    public IList<MuscleGroups> MuscleGroupsWithCore => MuscleGroups.Union(MuscleGroupExtensions.Core()).ToList();

    [NotMapped]
    public IList<MuscleGroups> MuscleGroupsSansCore => MuscleGroups.Except(MuscleGroupExtensions.Core()).ToList();
}
