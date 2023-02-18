using Microsoft.EntityFrameworkCore;
using Web.Code.Extensions;
using Web.Models.Exercise;

namespace Web.Entities.Newsletter;

/// <summary>
/// User's exercise routine history
/// </summary>
[Owned]
public record NewsletterRotation(int Id, MuscleGroups MuscleGroups, MovementPattern MovementPatterns)
{
    public string ToUserString()
    {
        return $"Day {Id}: ({MuscleGroups.GetSingleDisplayName(EnumExtensions.DisplayNameType.ShortName)}) {MovementPatterns.GetDisplayName32(EnumExtensions.DisplayNameType.ShortName)}";
    }
}
