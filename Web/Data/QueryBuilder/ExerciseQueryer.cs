using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Extensions;
using Web.Models.Exercise;
using Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using static Web.Data.QueryBuilder.ExerciseQueryBuilder;

namespace Web.Data.QueryBuilder;

public class ExerciseQueryer
{
    public record QueryResults(
        User? User,
        Exercise Exercise,
        Variation Variation,
        ExerciseVariation ExerciseVariation,
        UserExercise? UserExercise,
        UserExerciseVariation? UserExerciseVariation,
        UserVariation? UserVariation,
        Variation? EasierVariation,
        Variation? HarderVariation
    ) : IExerciseVariationCombo;

    [DebuggerDisplay("{Exercise}: {Variation}")]
    private class InProgressQueryResults :
        IExerciseVariationCombo
    {
        public Exercise Exercise { get; init; } = null!;
        public Variation Variation { get; init; } = null!;
        public ExerciseVariation ExerciseVariation { get; init; } = null!;
        public UserExercise? UserExercise { get; init; }
        public UserExerciseVariation? UserExerciseVariation { get; init; }
        public UserVariation? UserVariation { get; init; }
        public Variation? HarderVariation { get; init; }
        public Variation? EasierVariation { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
    }

    public readonly CoreContext Context;
    public readonly bool IgnoreGlobalQueryFilters = false;

    public required User? User;

    public required ExerciseType? ExerciseType;
    public required MuscleGroups? RecoveryMuscle;
    public required MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    public required bool? IncludeBonus;
    public required MuscleContractions? MuscleContractions;
    public required MuscleMovement? MuscleMovement;
    public required OrderByEnum OrderBy = OrderByEnum.None;
    public required SportsFocus? SportsFocus;
    public required int SkipCount = 0;
    public required bool? Unilateral = null;
    public required bool? AntiGravity = null;
    public required IEnumerable<int>? EquipmentIds;
    public required IEnumerable<int>? ExerciseExclusions;

    public required ProficiencyOptions Proficiency { get; init; }
    public required MovementPatternOptions MovementPattern { get; init; }
    public required MuscleGroupOptions MuscleGroup { get; init; }
    public required WeightOptions WeightOptions { get; init; }

    public ExerciseQueryer(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }


    /// <summary>
    /// Queries the db for the data
    /// </summary>
    public async Task<IList<QueryResults>> Query()
    {
        var eligibleExercisesQuery = Context.Exercises
            .Include(e => e.Prerequisites) // TODO Only necessary for the /exercises list, not the newsletter
                .ThenInclude(p => p.PrerequisiteExercise)
            .Select(i => new
            {
                Exercise = i,
                UserExercise = i.UserExercises.FirstOrDefault(ue => ue.User == User)
            })
            // Don't grab exercises that the user wants to ignore
            .Where(i => i.UserExercise == null || !i.UserExercise.Ignore)
            // Only show these exercises if the user has completed the previous reqs
            .Where(i => i.Exercise.Prerequisites
                    .Select(r => new
                    {
                        r.PrerequisiteExercise.Proficiency,
                        UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == User)
                    })
                    .All(p => User == null
                        || /* Require the prerequisites show first */ p.UserExercise != null
                            && (p.UserExercise.Ignore || p.UserExercise.Progression >= p.Proficiency)
                    )
            );

        var baseQuery = Context.Variations
            .AsNoTracking() // Don't update any entity
            .Include(i => i.Intensities)
            .Include(i => i.EquipmentGroups.Where(eg => eg.Parent == null).Where(eg => WeightOptions.PrefersWeights != false || !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight) || eg.Instruction != null)))
                // To display the equipment required for the exercise in the newsletter
                .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(i => i.EquipmentGroups.Where(eg => eg.Parent == null).Where(eg => WeightOptions.PrefersWeights != false || !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight) || eg.Instruction != null)))
                .ThenInclude(eg => eg.Children)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Join(Context.ExerciseVariations, o => o.Id, i => i.Variation.Id, (o, i) => new
            {
                Variation = o,
                ExerciseVariation = i
            })
            .Join(eligibleExercisesQuery, o => o.ExerciseVariation.Exercise.Id, i => i.Exercise.Id, (o, i) => new
            {
                o.Variation,
                o.ExerciseVariation,
                i.Exercise,
                i.UserExercise
            })
            .Where(vm => ExerciseExclusions == null ? true : !ExerciseExclusions.Contains(vm.Exercise.Id))
            .Where(vm => Proficiency.DoCapAtProficiency ? vm.ExerciseVariation.Progression.Min == null || vm.ExerciseVariation.Progression.Min <= vm.ExerciseVariation.Exercise.Proficiency : true)
            .Where(vm => Proficiency.CapAtUsersProficiencyPercent != null ? vm.ExerciseVariation.Progression.Min == null || vm.UserExercise == null || vm.ExerciseVariation.Progression.Min <= (vm.UserExercise.Progression * Proficiency.CapAtUsersProficiencyPercent) : true)
            .Select(a => new InProgressQueryResults()
            {
                UserExercise = a.UserExercise,
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == User),
                UserExerciseVariation = a.ExerciseVariation.UserExerciseVariations.FirstOrDefault(uv => uv.User == User),
                Exercise = a.Exercise,
                Variation = a.Variation,
                ExerciseVariation = a.ExerciseVariation,
                EasierVariation = Context.ExerciseVariations
                    .Where(ev => ev.ExerciseId == a.Exercise.Id)
                    .OrderByDescending(ev => ev.Progression.Max)
                    .First(ev => ev.Progression.Max != null && ev != a.ExerciseVariation && ev.Progression.Max <= (a.UserExercise == null ? UserExercise.MinUserProgression : a.UserExercise.Progression))
                    .Variation,
                HarderVariation = Context.ExerciseVariations
                    .Where(ev => ev.ExerciseId == a.Exercise.Id)
                    .OrderBy(ev => ev.Progression.Min)
                    .First(ev => ev.Progression.Min != null && ev != a.ExerciseVariation && ev.Progression.Min > (a.UserExercise == null ? UserExercise.MinUserProgression : a.UserExercise.Progression))
                    .Variation,
                IsMaxProgressionInRange = User != null && (
                    a.ExerciseVariation.Progression.Max == null
                    // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                    || a.UserExercise == null && UserExercise.MinUserProgression < a.ExerciseVariation.Progression.Max
                    // Compare the exercise's progression range with the user's exercise progression
                    || a.UserExercise != null && a.UserExercise!.Progression < a.ExerciseVariation.Progression.Max
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
                            || i.UserExercise == null && UserExercise.MinUserProgression >= i.ExerciseVariation.Progression.Min
                            // Compare the exercise's progression range with the user's exercise progression
                            || i.UserExercise != null && i.UserExercise!.Progression >= i.ExerciseVariation.Progression.Min);

            baseQuery = baseQuery.Where(i =>
                            // User owns at least one equipment in at least one of the optional equipment groups
                            i.Variation.EquipmentGroups.Any(eg => !eg.Equipment.Any())
                            || i.Variation.EquipmentGroups.Where(eg => eg.Equipment.Any()).Any(peg =>
                                peg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id))
                                && (
                                    !peg.Children.Any()
                                    || peg.Instruction != null // Exercise can be done without child equipment
                                    || peg.Children.Any(ceg => ceg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                                )
                            )
                        );
        }

        baseQuery = Filters.FilterMovementPattern(baseQuery, MovementPattern.MovementPatterns);
        baseQuery = Filters.FilterMuscleGroup(baseQuery, MuscleGroup.MuscleGroups, include: true, MuscleGroup.MuscleTarget);
        baseQuery = Filters.FilterMuscleGroup(baseQuery, MuscleGroup.ExcludeMuscleGroups, include: false, v => v.Variation.StrengthMuscles | v.Variation.StabilityMuscles);
        baseQuery = Filters.FilterEquipmentIds(baseQuery, EquipmentIds);
        baseQuery = Filters.FilterRecoveryMuscle(baseQuery, RecoveryMuscle);
        baseQuery = Filters.FilterSportsFocus(baseQuery, SportsFocus);
        baseQuery = Filters.FilterIncludeBonus(baseQuery, IncludeBonus);
        baseQuery = Filters.FilterAntiGravity(baseQuery, AntiGravity);
        baseQuery = Filters.FilterMuscleContractions(baseQuery, MuscleContractions);
        baseQuery = Filters.FilterMuscleMovement(baseQuery, MuscleMovement);
        baseQuery = Filters.FilterExerciseType(baseQuery, ExerciseType);
        baseQuery = Filters.FilterIsUnilateral(baseQuery, Unilateral);

        if (WeightOptions.OnlyWeights)
        {
            baseQuery = Filters.FilterOnlyWeights(baseQuery, WeightOptions.PrefersWeights);
        }
        else if (WeightOptions.PrefersWeights == false)
        {
            // TODO? Don't show an exercises weighted equipment groups if the user is not over the exercise's proficiency level?
            baseQuery = baseQuery.Where(vm => vm.Variation.EquipmentGroups.Any(eg => !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight))));
        }

        var queryResults = (await baseQuery.ToListAsync()).AsEnumerable();

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

        if (WeightOptions.PrefersWeights == true)
        {
            // User prefers weighted variations, order those next.
            // TODO? What about a variation that has two equipment groups, one bodyweight and one weighted? Is the exercise weighted?
            // TODO? Should we only look at equipment groups that the user owns when ordering by IsWeight?
            orderedResults = orderedResults.ThenByDescending(a => a.Variation.EquipmentGroups.Any(eg => eg.IsWeight));
        }

        orderedResults = orderedResults
            // Show variations that the user has rarely seen
            .ThenBy(a => a.UserExerciseVariation == null ? DateOnly.MinValue : a.UserExerciseVariation.LastSeen)
            // Mostly for the demo, show mostly random exercises
            .ThenBy(a => Guid.NewGuid());

        var muscleTarget = MuscleGroup.MuscleTarget.Compile();
        var finalResults = new List<QueryResults>();
        if (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null)
        {
            // Yikes
            while (MuscleGroup.AtLeastXUniqueMusclesPerExercise > 1)
            {
                if (OrderBy == OrderByEnum.UniqueMuscles)
                {
                    for (var i = 0; i < orderedResults.Count(); i++)
                    {
                        var primaryMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => finalResults.Sum(r => muscleTarget(r).HasFlag(v) ? 1 : 0));
                        var stack = orderedResults
                                        // The variation works at least x unworked muscles 
                                        .Where(vm => BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(vm).UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate((MuscleGroups)0, (curr, n) => curr | n.Key)))) <= BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups) - MuscleGroup.AtLeastXUniqueMusclesPerExercise)
                                        // Don't work a complex exercise as the last one
                                        .Where(vm => BitOperations.PopCount((ulong)muscleTarget(vm)) - BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(MusclesAlreadyWorked, (curr, n) => curr | n.Key))) <= 3)
                                        // Order by how many unique primary muscles the exercise works. After the least seen exercises, choose the optimal routine
                                        .OrderBy(vm => /*least seen:*/ i <= SkipCount ? 0 : BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(vm).UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(MusclesAlreadyWorked, (curr, n) => curr | n.Key)))))
                                        .ToList();

                        var exercise = stack.SkipWhile(e =>
                            // Ignore two variations from the same exercise, or two of the same variation
                            finalResults.Select(r => r.Exercise).Contains(e.Exercise)
                            // Two variations work the same muscles, ignore those
                            || finalResults.Any(fr => muscleTarget(fr) == muscleTarget(e))
                        ).FirstOrDefault();

                        if (exercise != null)
                        {
                            finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                        }
                    }
                }
                else
                {
                    foreach (var exercise in orderedResults)
                    {
                        if (finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                        {
                            continue;
                        }

                        var primaryMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => finalResults.Sum(r => muscleTarget(r).HasFlag(v) ? 1 : 0));
                        var allMusclesWorked = Enum.GetValues<MuscleGroups>().Where(e => BitOperations.PopCount((ulong)e) == 1).ToDictionary(k => k, v => finalResults.Sum(r => r.Variation.AllMuscles.HasFlag(v) ? 1 : 0));
                        // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                        if (BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(exercise).UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate((MuscleGroups)0, (curr, n) => curr | n.Key)))) <= BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups) - MuscleGroup.AtLeastXUniqueMusclesPerExercise
                            // Don't work a complex exercise as the last one
                            && BitOperations.PopCount((ulong)muscleTarget(exercise)) - BitOperations.PopCount((ulong)muscleTarget(exercise).UnsetFlag32(primaryMusclesWorked.Where(d => d.Value >= 1).Aggregate(MusclesAlreadyWorked, (curr, n) => curr | n.Key))) <= 3
                            // What does this do?
                            && BitOperations.PopCount((ulong)muscleTarget(exercise).UnsetFlag32(allMusclesWorked.Where(d => d.Value >= 2).Aggregate((MuscleGroups)0, (curr, n) => curr | n.Key))) > 0)
                        {
                            finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                        }
                    }
                }

                // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
                // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
                MuscleGroup.AtLeastXUniqueMusclesPerExercise--;
            }

            foreach (var exercise in orderedResults)
            {
                if (finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                {
                    continue;
                }

                var musclesWorkedSoFar = finalResults.WorkedMuscles();
                var allMusclesWorkedSoFar = finalResults.Aggregate(MusclesAlreadyWorked, (m, vm2) => m | muscleTarget(vm2));
                // Grab any muscle groups we missed in the previous loops. Include isolation exercises here
                if (MuscleGroup.AtLeastXUniqueMusclesPerExercise == 0 || muscleTarget(exercise).UnsetFlag32(musclesWorkedSoFar).HasAnyFlag32(MuscleGroup.MuscleGroups))
                {
                    finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                }
            }
        }
        else if (MovementPattern.MovementPatterns != null && MovementPattern.IsUnique)
        {
            foreach (var exercise in orderedResults)
            {
                // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                if (!finalResults.Aggregate((MovementPattern)0, (curr, n) => curr | n.Variation.MovementPattern).HasAnyFlag32(exercise.Variation.MovementPattern))
                {
                    finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                }
            }
        }
        else
        {
            finalResults = orderedResults.Select(a => new QueryResults(User, a.Exercise, a.Variation, a.ExerciseVariation, a.UserExercise, a.UserExerciseVariation, a.UserVariation, a.EasierVariation, a.HarderVariation)).ToList();
        }

        return OrderBy switch
        {
            OrderByEnum.None => finalResults,
            OrderByEnum.UniqueMuscles => finalResults,
            OrderByEnum.Progression => finalResults.Take(SkipCount).Concat(finalResults.Skip(SkipCount)
                                                   .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max))
                                                   .ToList(),
            OrderByEnum.MuscleTarget => finalResults.Take(SkipCount).Concat(finalResults.Skip(SkipCount)
                                                    .OrderByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)) - BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups)))
                                                    .ThenBy(vm => BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups))))
                                                    .ToList(),
            OrderByEnum.Name => finalResults.OrderBy(vm => vm.Exercise.Name)
                                            .ThenBy(vm => vm.Variation.Name)
                                            .ToList(),
            _ => finalResults,
        };
    }
}
