using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace FinerFettle.Web.Data
{
    public class ExerciseQueryBuilder
    {
        public enum OrderByEnum
        {
            None,
            Progression
        }

        public record QueryResults(User? User, Exercise Exercise, Variation Variation, ExerciseVariation ExerciseVariation, IntensityLevel? IntensityLevel);

        private readonly User? User;
        private readonly CoreContext Context;
        private ExerciseType? ExerciseType;
        private MuscleGroups? RecoveryMuscle;
        private MuscleGroups MuscleGroups;
        private bool IncludeRecoveryMuscle;
        private bool? PrefersWeights;
        private MuscleContractions? MuscleContractions;
        private IntensityLevel? IntensityLevel;
        private OrderByEnum OrderBy = OrderByEnum.None;
        private SportsFocus? SportsFocus;
        private int? TakeOut;
        private int? AtLeastXUniqueMusclesPerExercise;
        private bool DoCapAtProficiency = false;
        private bool IgnoreGlobalQueryFilters = false;

        public ExerciseQueryBuilder(CoreContext context, User? user, bool ignoreGlobalQueryFilters = false)
        {
            Context = context;
            User = user;
            IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
        }

        /// <summary>
        /// Filter exercises down to the specified type
        /// </summary>
        public ExerciseQueryBuilder WithExerciseType(ExerciseType exerciseType)
        {
            ExerciseType = exerciseType;
            return this;
        }

        /// <summary>
        /// If true, prefer weighted variations over bodyweight variations.
        /// If false, only show bodyweight variations.
        /// If null, show both weighted and bodyweight variations with equal precedence.
        /// </summary>
        public ExerciseQueryBuilder WithPrefersWeights(bool? prefersWeights)
        {
            PrefersWeights = prefersWeights;
            return this;
        }

        /// <summary>
        /// Return this many exercise variations
        /// </summary>
        public ExerciseQueryBuilder Take(int @out)
        {
            TakeOut = @out;
            return this;
        }

        /// <summary>
        /// Don't choose variations where the exercise min progression is greater than the exercise proficiency level.
        /// For things like warmups--rather have regular pushups over one-hand pushups.
        /// </summary>
        public ExerciseQueryBuilder CapAtProficiency(bool doCap)
        {
            DoCapAtProficiency = doCap;
            return this;
        }

        /// <summary>
        /// Filter variations down to these muscle contractions
        /// </summary>
        public ExerciseQueryBuilder WithMuscleContractions(MuscleContractions muscleContractions)
        {
            MuscleContractions = muscleContractions;
            return this;
        }

        /// <summary>
        /// Filter exercises down to these muscle groups
        /// </summary>
        public ExerciseQueryBuilder WithMuscleGroups(MuscleGroups muscleGroups)
        {
            MuscleGroups = muscleGroups;
            return this;
        }

        /// <summary>
        /// Filter exercises down to this intensity level
        /// </summary>
        public ExerciseQueryBuilder WithIntensityLevel(IntensityLevel intensityLevel)
        {
            IntensityLevel = intensityLevel;
            return this;
        }

        /// <summary>
        /// Order the final results
        /// </summary>
        public ExerciseQueryBuilder WithOrderBy(OrderByEnum orderBy)
        {
            OrderBy = orderBy;
            return this;
        }

        /// <summary>
        /// Filter exercises to where each exercise choosen works X unique muscle groups
        /// </summary>
        public ExerciseQueryBuilder WithAtLeastXUniqueMusclesPerExercise(int x)
        {
            AtLeastXUniqueMusclesPerExercise = x;
            return this;
        }

        /// <summary>
        /// Filter variations to the ones that target this spoirt
        /// </summary>
        public ExerciseQueryBuilder WithSportsFocus(SportsFocus sportsFocus)
        {
            SportsFocus = sportsFocus;
            return this;
        }

        /// <summary>
        /// Filer out exercises that touch on an injured muscle
        /// </summary>
        /// <param name="include">Include matching variations instead of excluding them</param>
        public ExerciseQueryBuilder WithRecoveryMuscle(MuscleGroups recoveryMuscle, bool include = false)
        {
            RecoveryMuscle = recoveryMuscle;
            IncludeRecoveryMuscle = include;
            return this;
        }

        /// <summary>
        /// Queries the db for the data
        /// </summary>
        public IList<QueryResults> Query()
        {
            var eligibleExercisesQuery = Context.Exercises
                .Include(e => e.Prerequisites) // TODO Only necessary for the /exercises list, not the newsletter
                    .ThenInclude(p => p.PrerequisiteExercise)
                .Select(i => new {
                    Exercise = i,
                    UserExercise = i.UserExercises.FirstOrDefault(ue => ue.User == User)
                })
                // Don't grab exercises that the user wants to ignore
                .Where(i => i.UserExercise == null || !i.UserExercise.Ignore)
                // Only show these exercises if the user has completed the previous reqs
                .Where(i => i.Exercise.Prerequisites
                        .Select(r => new { r.PrerequisiteExercise.Proficiency, UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == User) })
                        .All(p => p.UserExercise == null || p.UserExercise.Ignore || p.UserExercise.Progression >= p.Proficiency)
                );

            var baseQuery = Context.Variations
                .Include(i => i.Intensities)
                .Include(i => i.EquipmentGroups)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
                .Join(Context.ExerciseVariations, o => o.Id, i => i.Variation.Id, (o, i) => new { 
                    Variation = o, 
                    ExerciseVariation = i 
                })
                .Join(eligibleExercisesQuery, o => o.ExerciseVariation.Exercise.Id, i => i.Exercise.Id, (o, i) => new { 
                    o.Variation, 
                    o.ExerciseVariation, 
                    i.Exercise, 
                    i.UserExercise
                })
                .Where(vm => DoCapAtProficiency ? (vm.ExerciseVariation.Progression.Min == null || vm.ExerciseVariation.Progression.Min <= vm.ExerciseVariation.Exercise.Proficiency) : true)
                .Select(a => new
                {
                    a.Variation,
                    a.ExerciseVariation,
                    a.UserExercise,
                    a.Exercise,
                    UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == User),
                    IsMaxProgressionInRange = User != null && (
                            a.ExerciseVariation.Progression.Max == null
                            // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                            || (a.UserExercise == null && (UserExercise.MinUserProgression < a.ExerciseVariation.Progression.Max))
                            // Compare the exercise's progression range with the user's exercise progression
                            || (a.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(a.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) < a.ExerciseVariation.Progression.Max)
                        )
                });

            if (IgnoreGlobalQueryFilters)
            {
                baseQuery = baseQuery.IgnoreQueryFilters();
            }

            if (User != null)
            {
                baseQuery = baseQuery.Where(i => i.ExerciseVariation.Progression.Min == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (UserExercise.MinUserProgression >= i.ExerciseVariation.Progression.Min))
                                // Compare the exercise's progression range with the user's exercise progression
                                || (i.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Floor(i.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) >= i.ExerciseVariation.Progression.Min));

                baseQuery = baseQuery.Where(i => (
                                // User owns at least one equipment in at least one of the optional equipment groups
                                !i.Variation.EquipmentGroups.Any(eg => !eg.Required && eg.Equipment.Any())
                                || i.Variation.EquipmentGroups.Where(eg => !eg.Required && eg.Equipment.Any()).Any(eg => eg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                            ) && (
                                // User owns at least one equipment in all of the required equipment groups
                                !i.Variation.EquipmentGroups.Any(eg => eg.Required && eg.Equipment.Any())
                                || i.Variation.EquipmentGroups.Where(eg => eg.Required && eg.Equipment.Any()).All(eg => eg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                            ));
            }

            if (RecoveryMuscle != null && RecoveryMuscle != MuscleGroups.None)
            {
                if (IncludeRecoveryMuscle)
                {
                    baseQuery = baseQuery
                        .Where(i => i.Exercise.IsRecovery)
                        .Where(i => (i.Exercise.PrimaryMuscles & RecoveryMuscle.Value) != 0);
                }
                else
                {
                    // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                    baseQuery = baseQuery
                        .Where(i => !i.Exercise.IsRecovery)
                        .Where(i => !(((i.Exercise.PrimaryMuscles | i.Exercise.SecondaryMuscles) & RecoveryMuscle.Value) != 0));
                }
            } 
            else if (RecoveryMuscle != null)
            {
                baseQuery = baseQuery.Where(i => !i.Exercise.IsRecovery);
            }

            if (SportsFocus != null)
            {
                baseQuery = baseQuery.Where(vm => vm.Variation.SportsFocus.HasFlag(SportsFocus.Value));
            }

            if (MuscleContractions != null)
            {
                if (MuscleContractions.Value == Models.Exercise.MuscleContractions.Isometric)
                {
                    baseQuery = baseQuery.Where(vm => vm.Variation.MuscleContractions == MuscleContractions.Value);
                }
                else
                {
                    baseQuery = baseQuery.Where(vm => vm.Variation.MuscleContractions.HasFlag(MuscleContractions.Value));
                }
            }

            if (ExerciseType != null)
            {
                // Make sure the exercise is for the correct workout type
                baseQuery = baseQuery.Where(vm => (vm.Variation.ExerciseType & ExerciseType.Value) != 0);
            }

            if (IntensityLevel != null)
            {
                // Make sure the exercise has an intensity
                baseQuery = baseQuery.Where(vm => vm.Variation.Intensities.Any(i => i.IntensityLevel == IntensityLevel));
            }

            if (PrefersWeights == false)
            {
                baseQuery = baseQuery.Where(vm => !vm.Variation.EquipmentGroups.Any(eg => eg.IsWeight));
            }

            var queryResults = baseQuery.ToList().AsEnumerable();

            if (User != null)
            {
                // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
                queryResults = queryResults.GroupBy(i => new { i.Exercise.Id })
                                    .Select(g => new
                                    {
                                        g.Key,
                                        // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                        Variations = g.Where(a => a.IsMaxProgressionInRange).NullIfEmpty() ?? g.Where(a => !a.IsMaxProgressionInRange).OrderByDescending(a => a.ExerciseVariation.Progression.GetMaxOrDefault).Take(1)
                                    })
                                    .SelectMany(g => g.Variations);
            }

            // OrderBy must come after query or you get duplicates
            // Show exercises that the user has rarely seen
            var orderedResults = queryResults.OrderBy(a => a.UserExercise == null ? DateOnly.MinValue : a.UserExercise.LastSeen);

            if (PrefersWeights == true)
            {
                // User prefers weighted variations, order those next
                orderedResults = orderedResults.ThenByDescending(a => a.Variation.EquipmentGroups.Any(eg => eg.IsWeight));
            }

            orderedResults = orderedResults
                // Show variations that the user has rarely seen
                .ThenBy(a => a.UserVariation == null ? DateOnly.MinValue : a.UserVariation.LastSeen)
                // Mostly for the demo, show mostly random exercises
                .ThenBy(a => Guid.NewGuid());

            var finalResults = new List<QueryResults>();
            if (AtLeastXUniqueMusclesPerExercise != null)
            {
                if (MuscleGroups == MuscleGroups.None)
                {
                    throw new ArgumentNullException(nameof(MuscleGroups));
                }

                if (AtLeastXUniqueMusclesPerExercise > BitOperations.PopCount((ulong)MuscleGroups))
                {
                    throw new ArgumentOutOfRangeException(nameof(AtLeastXUniqueMusclesPerExercise));
                }

                while (AtLeastXUniqueMusclesPerExercise > 1)
                {
                    foreach (var exercise in orderedResults)
                    {
                        var musclesWorkedSoFar = finalResults.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles);
                        // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                        if (BitOperations.PopCount((ulong)MuscleGroups.UnsetFlag32(exercise.Exercise.PrimaryMuscles.UnsetFlag32(musclesWorkedSoFar))) <= (BitOperations.PopCount((ulong)MuscleGroups) - AtLeastXUniqueMusclesPerExercise))
                        {
                            finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, IntensityLevel));
                        }
                    }

                    // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
                    // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
                    AtLeastXUniqueMusclesPerExercise--;
                }
                
                foreach (var exercise in orderedResults)
                {
                    var musclesWorkedSoFar = finalResults.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles);
                    // Grab any muscle groups we missed in the previous loops. Include isolation exercises here
                    if (exercise.Exercise.PrimaryMuscles.UnsetFlag32(musclesWorkedSoFar).HasAnyFlag32(MuscleGroups))
                    {
                        finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, IntensityLevel));
                    }
                }
            } 
            else
            {
                finalResults = orderedResults.Select(a => new QueryResults(User, a.Exercise, a.Variation, a.ExerciseVariation, IntensityLevel)).ToList();
            }

            if (TakeOut != null)
            {
                finalResults = finalResults.Take(TakeOut.Value).ToList();
            }

            switch (OrderBy)
            {
                case OrderByEnum.Progression:
                    finalResults = finalResults.OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                        .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                        .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
                        .ToList();
                    break;
                default: break;
            }

            return finalResults;
        }
    }
}
