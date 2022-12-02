using System.Numerics;
using Web.Data.QueryBuilder;
using Web.Entities.Exercise;
using Web.Models.Exercise;

namespace Web.Extensions;

public static class VariationExtensions
{
    public static MuscleGroups WorkedMuscles<T>(this IList<T> list, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, MuscleGroups? addition = null) where T : IExerciseVariationCombo
    {
        return list.Aggregate(addition ?? MuscleGroups.None, (acc, curr) => acc | muscleTarget(curr));
    }

    public static IDictionary<MuscleGroups, int> WorkedMusclesDict<T>(this IList<T> list, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, IDictionary<MuscleGroups, int>? addition = null) where T : IExerciseVariationCombo
    {
        return Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => (addition?[v] ?? 0) + list.Sum(r => muscleTarget(r).HasFlag(v) ? 1 : 0));
    }
}
