using FinerFettle.Web.Entities.Exercise;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.Data;

public interface IQueryFiltersSportsFocus
{
    Exercise Exercise { get; }
    Variation Variation { get; }
}

public interface IQueryFiltersExerciseType
{
    Variation Variation { get; }
}

public interface IQueryFiltersIntensityLevel
{
    Variation Variation { get; }
}

public interface IQueryFiltersOnlyWeights
{
    Variation Variation { get; }
}

public interface IQueryFiltersMuscleContractions
{
    Variation Variation { get; }
}

public interface IQueryFiltersMuscleMovement
{
    Variation Variation { get; }
}

public interface IQueryFiltersEquipmentIds
{
    Variation Variation { get; }
}

public interface IQueryFiltersRecoveryMuscle
{
    Exercise Exercise { get; }
    Variation Variation { get; }
}

public interface IQueryFiltersShowCore
{
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
    public static IQueryable<T> FilterSportsFocus<T>(IQueryable<T> query, SportsFocus? sportsFocus) where T : IQueryFiltersSportsFocus
    {
        if (sportsFocus != null)
        {
            query = query.Where(i => i.Exercise.SportsFocus == sportsFocus);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise is for the correct workout type
    /// </summary>
    public static IQueryable<T> FilterExerciseType<T>(IQueryable<T> query, ExerciseType? exerciseType) where T : IQueryFiltersExerciseType
    {
        if (exerciseType != null)
        {
            query = query.Where(vm => (vm.Variation.ExerciseType & exerciseType.Value) != 0);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterIntensityLevel<T>(IQueryable<T> query, IntensityLevel? intensityLevel) where T : IQueryFiltersIntensityLevel
    {
        if (intensityLevel != null)
        {
            query = query.Where(vm => vm.Variation.Intensities.Any(i => i.IntensityLevel == intensityLevel));
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterOnlyWeights<T>(IQueryable<T> query, bool? onlyWeights) where T : IQueryFiltersOnlyWeights
    {
        if (onlyWeights != null)
        {
            query = query.Where(vm => vm.Variation.EquipmentGroups.Any(eg => eg.IsWeight) == onlyWeights.Value);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMuscleContractions<T>(IQueryable<T> query, MuscleContractions? muscleContractions) where T : IQueryFiltersMuscleContractions
    {
        if (muscleContractions != null)
        {
            if (muscleContractions.Value == MuscleContractions.Static)
            {
                query = query.Where(vm => vm.Variation.MuscleContractions == muscleContractions.Value);
            }
            else
            {
                query = query.Where(vm => vm.Variation.MuscleContractions.HasFlag(muscleContractions.Value));
            }
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise has an intensity
    /// </summary>
    public static IQueryable<T> FilterMuscleMovement<T>(IQueryable<T> query, MuscleMovement? muscleMovement) where T : IQueryFiltersMuscleContractions
    {
        if (muscleMovement != null)
        {
            query = query.Where(vm => vm.Variation.MuscleMovement == muscleMovement.Value);
        }

        return query;
    }

    /// <summary>
    /// Make sure the exercise works a specific muscle group
    /// </summary>
    public static IQueryable<T> FilterRecoveryMuscle<T>(IQueryable<T> query, MuscleGroups? recoveryMuscle, bool include = false) where T : IQueryFiltersRecoveryMuscle
    {
        if (recoveryMuscle != null && recoveryMuscle != MuscleGroups.None)
        {
            if (include)
            {
                query = query
                    .Where(i => i.Exercise.IsRecovery)
                    .Where(i => (i.Variation.PrimaryMuscles & recoveryMuscle.Value) != 0);
            }
            else
            {
                // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                query = query
                    .Where(i => !i.Exercise.IsRecovery)
                    .Where(i => !(((i.Variation.PrimaryMuscles | i.Variation.SecondaryMuscles) & recoveryMuscle.Value) != 0));
            }
        }
        else if (recoveryMuscle != null)
        {
            query = query.Where(i => !i.Exercise.IsRecovery);
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
    public static IQueryable<T> FilterIncludeBonus<T>(IQueryable<T> query, bool? includeBonus) where T : IQueryFiltersShowCore
    {
        if (includeBonus != null)
        {
            query = query.Where(vm => vm.ExerciseVariation.IsBonus == includeBonus);
        }

        return query;
    }

    /// <summary>
    ///     Filters exercises to whether they use certain equipment.
    /// </summary>
    public static IQueryable<T> FilterEquipmentIds<T>(IQueryable<T> query, IEnumerable<int>? equipmentIds) where T : IQueryFiltersEquipmentIds
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
