using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.Data
{
    public interface IQueryFiltersSportsFocus
    {
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

    public interface IQueryFiltersMuscleContractions
    {
        Variation Variation { get; }
    }

    public interface IQueryFiltersRecoveryMuscle
    {
        Exercise Exercise { get; }
        Variation Variation { get; }
    }

    public static class Filters
    {
        /// <summary>
        /// Filter exercises to ones that help with a specific sport
        /// </summary>
        public static IQueryable<T> FilterSportsFocus<T>(IQueryable<T> query, SportsFocus? sportsFocus) where T : IQueryFiltersSportsFocus
        {
            if (sportsFocus != null)
            {
                return query.Where(q => q.Variation.SportsFocus.HasFlag(sportsFocus));
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
        public static IQueryable<T> FilterMuscleContractions<T>(IQueryable<T> query, MuscleContractions? muscleContractions) where T : IQueryFiltersMuscleContractions
        {
            if (muscleContractions != null)
            {
                if (muscleContractions.Value == MuscleContractions.Isometric)
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
                        .Where(i => (i.Exercise.PrimaryMuscles & recoveryMuscle.Value) != 0);
                }
                else
                {
                    // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                    query = query
                        .Where(i => !i.Exercise.IsRecovery)
                        .Where(i => !(((i.Exercise.PrimaryMuscles | i.Exercise.SecondaryMuscles) & recoveryMuscle.Value) != 0));
                }
            }
            else if (recoveryMuscle != null)
            {
                query = query.Where(i => !i.Exercise.IsRecovery);
            }

            return query;
        }
    }
}
