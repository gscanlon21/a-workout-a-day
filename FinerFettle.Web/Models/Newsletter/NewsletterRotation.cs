using FinerFettle.Web.Models.Exercise;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Models.Newsletter
{
    /// <summary>
    /// User's exercise routine history
    /// </summary>
    [Owned]
    public record NewsletterRotation(int Id, ExerciseType ExerciseType, IntensityLevel IntensityLevel, MuscleGroups MuscleGroups);
}
