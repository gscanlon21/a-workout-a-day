using System.Linq.Expressions;
using System.Reflection;
using Web.Entities.Exercise;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Data.QueryBuilder;

public interface IExerciseVariationCombo
{
    Exercise Exercise { get; }
    Variation Variation { get; }
    ExerciseVariation ExerciseVariation { get; }
}

public static class Filters
{
    /// <summary>
    ///     Filter exercises to ones that help with a specific sport
    /// </summary>
    /// <param name="sportsFocus">
    ///     If null, does not filter the query.
    ///     If SportsFocus.None, filters the query down to exercises that don't target a sport..
    ///     If > SportsFocus.None, filters the query down to exercises that target that specific sport.
    /// </param>
    public static IQueryable<T> FilterSportsFocus<T>(IQueryable<T> query, SportsFocus? sportsFocus) where T : IExerciseVariationCombo
    {
        if (sportsFocus.HasValue)
        {
            query = query.Where(i => i.Exercise.SportsFocus == sportsFocus);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise is for the correct workout type
    /// </summary>
    public static IQueryable<T> FilterExerciseType<T>(IQueryable<T> query, ExerciseType? exerciseType) where T : IExerciseVariationCombo
    {
        if (exerciseType.HasValue)
        {
            query = query.Where(vm => (vm.ExerciseVariation.ExerciseType & exerciseType.Value) != 0);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterIntensityLevel<T>(IQueryable<T> query, IntensityLevel? intensityLevel) where T : IExerciseVariationCombo
    {
        if (intensityLevel.HasValue)
        {
            query = query.Where(vm => vm.Variation.Intensities.Any(i => i.IntensityLevel == intensityLevel));
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterOnlyWeights<T>(IQueryable<T> query, bool? onlyWeights) where T : IExerciseVariationCombo
    {
        if (onlyWeights.HasValue)
        {
            query = query.Where(vm => vm.Variation.EquipmentGroups.Any(eg => eg.IsWeight) == onlyWeights.Value);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterIsUnilateral<T>(IQueryable<T> query, bool? isUnilateral) where T : IExerciseVariationCombo
    {
        if (isUnilateral.HasValue)
        {
            query = query.Where(vm => vm.Variation.Unilateral == isUnilateral.Value);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMuscleContractions<T>(IQueryable<T> query, MuscleContractions? muscleContractions) where T : IExerciseVariationCombo
    {
        if (muscleContractions.HasValue)
        {
            if (muscleContractions.Value == MuscleContractions.Static)
            {
                query = query.Where(vm => vm.Variation.MuscleContractions == muscleContractions.Value);
            }
            else
            {
                query = query.Where(vm => (vm.Variation.MuscleContractions & muscleContractions.Value) != 0);
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMuscleMovement<T>(IQueryable<T> query, MuscleMovement? muscleMovement) where T : IExerciseVariationCombo
    {
        if (muscleMovement.HasValue)
        {
            query = query.Where(vm => (vm.Variation.MuscleMovement & muscleMovement.Value) != 0);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMovementPattern<T>(IQueryable<T> query, MovementPattern? muscleMovement) where T : IExerciseVariationCombo
    {
        if (muscleMovement.HasValue)
        {
            if (muscleMovement == MovementPattern.None)
            {
                query = query.Where(vm => vm.Variation.MovementPattern == MovementPattern.None);
            }
            else
            {
                query = query.Where(vm => (vm.Variation.MovementPattern & muscleMovement.Value) != 0);
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise works a specific muscle group
    /// </summary>
    public static IQueryable<T> FilterRecoveryMuscle<T>(IQueryable<T> query, MuscleGroups? recoveryMuscle) where T : IExerciseVariationCombo
    {
        if (recoveryMuscle.HasValue)
        {
            query = query.Where(i => i.Exercise.RecoveryMuscle == recoveryMuscle);
        }

        return query;
    }

    private static IQueryable<T> WithMuscleTarget<T>(this IQueryable<T> entities,
        Expression<Func<IExerciseVariationCombo, MuscleGroups>> propertySelector, MuscleGroups muscleGroup, bool include)
    {
        var variationExpr = (MemberExpression)((MemberExpression)propertySelector.Body).Expression;
        var variationProp = (PropertyInfo)variationExpr.Member;
        var musclesProp = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;

        ParameterExpression parameter = Expression.Parameter(typeof(T));

        var expression = Expression.Lambda<Func<T, bool>>(
            Expression.Equal(Expression.NotEqual(
            Expression.And(
                Expression.Convert(Expression.Property(Expression.Property(parameter, variationProp), musclesProp), typeof(int)),
                Expression.Convert(Expression.Constant(muscleGroup), typeof(int))
            ),
            Expression.Constant(0)), Expression.Constant(include)),
            parameter);

        return entities.Where(expression);
    }

    /// <summary>
    /// Make sure the exercise works a specific muscle group
    /// </summary>
    public static IQueryable<T> FilterMuscleGroup<T>(IQueryable<T> query, MuscleGroups? muscleGroup, bool include, Expression<Func<IExerciseVariationCombo, MuscleGroups>> muscleTarget) where T : IExerciseVariationCombo
    {
        if (muscleGroup.HasValue && muscleGroup != MuscleGroups.None)
        {
            if (include)
            {
                query = Filters.WithMuscleTarget(query, muscleTarget, muscleGroup.Value, include);
            }
            else
            {
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                query = Filters.WithMuscleTarget(query, muscleTarget, muscleGroup.Value, include);
            }
        }

        return query;
    }

    /// <summary>
    ///     Filters exercises to whether they are obscure or not.
    /// </summary>
    /// <param name="includeBonus">
    ///     If null, the query will not be filtered.
    ///     If true, the query will be filtered to only exercises that are bonus exercises.
    ///     If false, the query will be filtered to only exercises that are not bonus exercises.
    /// </param>
    public static IQueryable<T> FilterIncludeBonus<T>(IQueryable<T> query, bool? includeBonus) where T : IExerciseVariationCombo
    {
        if (includeBonus.HasValue)
        {
            query = query.Where(vm => vm.ExerciseVariation.IsBonus == includeBonus);
        }

        return query;
    }

    public static IQueryable<T> FilterAntiGravity<T>(IQueryable<T> query, bool? antiGravity) where T : IExerciseVariationCombo
    {
        if (antiGravity.HasValue)
        {
            query = query.Where(vm => vm.Variation.AntiGravity == antiGravity);
        }

        return query;
    }

    /// <summary>
    ///     Filters exercises to whether they use certain equipment.
    /// </summary>
    public static IQueryable<T> FilterEquipmentIds<T>(IQueryable<T> query, IEnumerable<int>? equipmentIds) where T : IExerciseVariationCombo
    {
        if (equipmentIds != null)
        {
            if (equipmentIds.Any())
            {
                query = query.Where(i => i.Variation.EquipmentGroups.Where(eg => eg.Equipment.Any()).Any(eg => eg.Equipment.Any(e => equipmentIds.Contains(e.Id))));
            }
            else
            {
                query = query.Where(i => !i.Variation.EquipmentGroups.Any(eg => eg.Equipment.Any()));
            }
        }

        return query;
    }
}
