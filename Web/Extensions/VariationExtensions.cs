using Web.Data.QueryBuilder;
using Web.Entities.Exercise;
using Web.Models.Exercise;

namespace Web.Extensions;

public static class VariationExtensions
{
    public static MuscleGroups WorkedMuscles<T>(this IList<T> list, MuscleGroups? addition = null, Func<Variation, MuscleGroups>? muscleTarget = null) where T : IExerciseVariationCombo
    {
        return list.Aggregate(addition ?? MuscleGroups.None, (acc, curr) => acc | (muscleTarget?.Invoke(curr.Variation) ?? curr.Variation.StrengthMuscles));
    }
}
