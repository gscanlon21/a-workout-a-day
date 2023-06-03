using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using Web.Code.Extensions;
using Web.Models.Exercise;

namespace Web.Entities.Newsletter;

/// <summary>
/// User's exercise routine history
/// </summary>
[Owned]
public record NewsletterRotation(int Id, MuscleGroups MuscleGroups, MovementPattern MovementPatterns)
{
    public string ToUserString(bool includeDay = true)
    {
        return $"{(includeDay ? $"Day {Id}: " : "")}({MuscleGroups.GetSingleDisplayName(EnumExtensions.DisplayNameType.ShortName)}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayNameType.ShortName)}";
    }

    [NotMapped]
    public MuscleGroups MuscleGroupsWithCore = MuscleGroups | MuscleGroups.Core;

    [NotMapped]
    public MuscleGroups MuscleGroupsSansCore = MuscleGroups.UnsetFlag32(MuscleGroups.Core);

    [NotMapped]
    public MuscleGroups StretchingMuscleGroups = MuscleGroups.UnsetFlag32(MuscleGroups.DoNotStretch);

    [NotMapped]
    public bool IsFullBody => MuscleGroups == MuscleGroups.UpperLower;
}
