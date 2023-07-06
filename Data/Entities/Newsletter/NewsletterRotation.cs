using Core.Code.Extensions;
using Core.Models.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities.Newsletter;

/// <summary>
/// A day of a user's workout split.
/// </summary>
[Owned]
public record WorkoutRotation(int Id, MuscleGroups MuscleGroups, MovementPattern MovementPatterns)
{
    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroups.GetDisplayName322(EnumExtensions.DisplayNameType.ShortName)}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayNameType.ShortName)}";
    }

    [NotMapped]
    public MuscleGroups MuscleGroupsWithCore = MuscleGroups | MuscleGroups.Core;

    [NotMapped]
    public MuscleGroups MuscleGroupsSansCore = MuscleGroups.UnsetFlag32(MuscleGroups.Core);
}
