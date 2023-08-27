using Core.Code.Extensions;
using Core.Models.Exercise;
using Data.Query;

namespace Data.Code.Extensions;

public static class VariationExtensions
{
    /// <summary>
    /// Returns the bitwise ORed result of muscles targeted by any of the items in the list.
    /// </summary>
    public static MuscleGroups WorkedMuscles<T>(this IEnumerable<T> list, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, MuscleGroups? addition = null) where T : IExerciseVariationCombo
    {
        return list.Aggregate(addition ?? MuscleGroups.None, (acc, curr) => acc | muscleTarget(curr));
    }

    /// <summary>
    /// Returns the muscles targeted by any of the items in the list as a dictionary with their count of how often they occur.
    /// </summary>
    public static IDictionary<MuscleGroups, int> WorkedMusclesDict<T>(this IEnumerable<T> list, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, int weightDivisor = 1, IDictionary<MuscleGroups, int>? addition = null) where T : IExerciseVariationCombo
    {
        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(k => k, v => ((addition?.TryGetValue(v, out int s) ?? false) ? s : 0) + list.Sum(r => muscleTarget(r).HasFlag(v) ? (1 / weightDivisor) : 0));
    }

    /// <summary>
    /// Returns the muscles targeted by any of the items in the list as a dictionary with their count of how often they occur.
    /// </summary>
    public static IDictionary<MuscleGroups, int> WorkedMusclesDict<T>(this IEnumerable<T> list, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, MuscleGroups addition) where T : IExerciseVariationCombo
    {
        return EnumExtensions.GetSingleValues32<MuscleGroups>().ToDictionary(k => k, v => (addition.HasFlag(v) ? 1 : 0) + list.Sum(r => muscleTarget(r).HasFlag(v) ? 1 : 0));
    }
}
