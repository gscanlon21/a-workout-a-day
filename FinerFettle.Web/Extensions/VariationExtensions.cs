using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.Extensions;

public static class VariationExtensions
{
    public static MuscleGroups WorkedMuscles<T>(this IList<T> list, MuscleGroups? addition = null) where T : IExerciseVariationCombo
    {
        return list.Aggregate(addition ?? MuscleGroups.None, (acc, curr) => acc | curr.Variation.PrimaryMuscles);
    }
}
