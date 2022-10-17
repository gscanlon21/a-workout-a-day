using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.Numerics;

namespace FinerFettle.Web.Data
{
    public class ExerciseQueryBuilder
    {
        private readonly User? User;
        private readonly CoreContext Context;
        private readonly bool? Demo;
        private ExerciseType? ExerciseType;
        private MuscleGroups? RecoveryMuscle;
        private MuscleGroups MuscleGroups;
        private bool IncludeRecoveryMuscle;
        private bool? PrefersWeights;
        private MuscleContractions? MuscleContractions;
        private IntensityLevel? IntensityLevel;
        private ExerciseActivityLevel ActivityLevel;
        private SportsFocus? SportsFocus;
        private int? TakeOut;
        private int? AtLeastXUniqueMusclesPerExercise;
        private bool DoCapAtProficiency = false;

        public ExerciseQueryBuilder(CoreContext context, User? user, bool? demo = null)
        {
            Context = context;
            User = user;
            Demo = demo;
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
        /// Filter exercises down to the specified activity level
        /// </summary>
        public ExerciseQueryBuilder WithActivityLevel(ExerciseActivityLevel activityLevel)
        {
            ActivityLevel = activityLevel;
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

        public IList<ExerciseViewModel> Build(string token)
        {
            var baseQuery = Context.Variations
                .Include(i => i.Intensities)
                .Include(v => v.Exercise)
                .Include(i => i.EquipmentGroups)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
                // Select the current progression of each exercise
                .Select(i => new {
                    Variation = i,
                    UserVariation = i.UserVariations.FirstOrDefault(uv => uv.User == User),
                    UserExercise = i.Exercise.UserExercises.FirstOrDefault(ue => ue.User == User)
                })
                // Don't grab exercises that the user wants to ignore
                .Where(i => i.UserExercise == null || !i.UserExercise.Ignore)
                // Only show these exercises if the user has completed the previous reqs
                .Where(i => i.Variation.Exercise.Prerequisites
                                .Select(r => new { r.PrerequisiteExercise.Proficiency, UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == User) })
                                .All(p => p.UserExercise == null || p.UserExercise.Ignore || p.UserExercise.Progression >= p.Proficiency)
                )
                .Where(vm => DoCapAtProficiency ? (vm.Variation.Progression.Min == null || vm.Variation.Progression.Min <= vm.Variation.Exercise.Proficiency) : true)
                .Select(a => new
                {
                    a.Variation,
                    a.UserExercise,
                    a.UserVariation,
                    IsMaxProgressionInRange = User != null && (
                            a.Variation.Progression.Max == null
                            // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                            || (a.UserExercise == null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(User.AverageProgression / (double)UserExercise.RoundToNearestX) < a.Variation.Progression.Max))
                            // Compare the exercise's progression range with the user's exercise progression
                            || (a.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(a.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) < a.Variation.Progression.Max)
                        )
                });

            if (User != null)
            {
                baseQuery = baseQuery.Where(i => i.Variation.Progression.Min == null
                                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                                || (i.UserExercise == null && (UserExercise.RoundToNearestX * (int)Math.Floor(User.AverageProgression / (double)UserExercise.RoundToNearestX) >= i.Variation.Progression.Min))
                                // Compare the exercise's progression range with the user's exercise progression
                                || (i.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Floor(i.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) >= i.Variation.Progression.Min));
                //            .Where(i => i.Variation.Progression.Max == null
                //                // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                //                || (i.UserExercise == null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(User.AverageProgression / (double)UserExercise.RoundToNearestX) < i.Variation.Progression.Max))
                //                // Compare the exercise's progression range with the user's exercise progression
                //                || (i.UserExercise != null && (UserExercise.RoundToNearestX * (int)Math.Ceiling(i.UserExercise!.Progression / (double)UserExercise.RoundToNearestX)) < i.Variation.Progression.Max));

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
                    baseQuery = baseQuery.Where(i => i.Variation.Exercise.PrimaryMuscles.HasFlag(RecoveryMuscle.Value));
                }
                else
                {
                    // If a recovery muscle is set, don't choose any exercises that work the injured muscle
                    baseQuery = baseQuery.Where(i => !(i.Variation.Exercise.PrimaryMuscles | i.Variation.Exercise.SecondaryMuscles).HasFlag(RecoveryMuscle.Value));
                }
            }

            if (SportsFocus != null)
            {
                baseQuery = baseQuery.Where(vm => vm.Variation.SportsFocus.HasFlag(SportsFocus.Value));
            }

            if (MuscleContractions != null)
            {
                baseQuery = baseQuery.Where(vm => vm.Variation.MuscleContractions.HasFlag(MuscleContractions.Value));
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
                queryResults = queryResults.GroupBy(i => new { i.Variation.Exercise.Id })
                                    .Select(g => new
                                    {
                                        g.Key,
                                        // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                        Variations = g.Where(a => a.IsMaxProgressionInRange).NullIfEmpty() ?? g.Where(a => !a.IsMaxProgressionInRange).OrderByDescending(a => a.Variation.Progression.GetMaxOrDefault).Take(1)
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

            var finalResults = new List<Variation>();
            if (AtLeastXUniqueMusclesPerExercise != null)
            {
                if (MuscleGroups == MuscleGroups.None)
                {
                    throw new ArgumentNullException(nameof(MuscleGroups));
                }

                foreach (var exercise in orderedResults)
                {
                    var musclesWorkedSoFar = finalResults.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles);
                    // Choose either compound exercises that cover at least two muscles in the targeted muscles set
                    if (BitOperations.PopCount((ulong)MuscleGroups.UnsetFlag32(exercise.Variation.Exercise.PrimaryMuscles.UnsetFlag32(musclesWorkedSoFar))) <= (BitOperations.PopCount((ulong)MuscleGroups) - AtLeastXUniqueMusclesPerExercise))
                    {
                        finalResults.Add(exercise.Variation);
                    }
                }

                foreach (var exercise in orderedResults)
                {
                    var musclesWorkedSoFar = finalResults.Aggregate((MuscleGroups)0, (m, vm2) => m | vm2.Exercise.PrimaryMuscles);
                    // Grab any muscle groups we missed in the previous aggregate. Include isolation exercises here
                    if (exercise.Variation.Exercise.PrimaryMuscles.UnsetFlag32(musclesWorkedSoFar).HasAnyFlag32(MuscleGroups))
                    {
                        finalResults.Add(exercise.Variation);
                    }
                }
            } 
            else
            {
                finalResults = orderedResults.DistinctBy(r => r.Variation.Exercise.Id).Select(a => a.Variation).ToList();
            }

            if (TakeOut != null)
            {
                finalResults = finalResults.Take(TakeOut.Value).ToList();
            }

            return finalResults.Select(i => new ExerciseViewModel(User != null ? new ViewModels.User.UserNewsletterViewModel(User, token) : null, i, intensityLevel: IntensityLevel, activityLevel: ActivityLevel)
            {
                Demo = Demo
            }).ToList();
        }
    }
}
