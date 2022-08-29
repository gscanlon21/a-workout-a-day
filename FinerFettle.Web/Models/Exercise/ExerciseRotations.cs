using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// User's exercise routine history
    /// </summary>
    [Owned]
    public record ExerciseRotaion(int id, ExerciseType ExerciseType, MuscleGroups MuscleGroups);
}
