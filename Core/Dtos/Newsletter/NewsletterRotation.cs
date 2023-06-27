using Core.Code.Extensions;
using Core.Models.Exercise;


namespace Core.Dtos.Newsletter;

/// <summary>
/// A day of a user's workout split.
/// </summary>
public record WorkoutRotation(int Id, MuscleGroups MuscleGroups, MovementPattern MovementPatterns)
{
    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroups.GetDisplayName322(EnumExtensions.DisplayNameType.ShortName)}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayNameType.ShortName)}";
    }

    public MuscleGroups MuscleGroupsWithCore = MuscleGroups | MuscleGroups.Core;

    public MuscleGroups MuscleGroupsSansCore = MuscleGroups.UnsetFlag32(MuscleGroups.Core);

    public bool IsFullBody => MuscleGroups == MuscleGroups.UpperLower;
}
