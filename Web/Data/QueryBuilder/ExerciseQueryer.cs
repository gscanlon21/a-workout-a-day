using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Numerics;
using static Web.Data.QueryBuilder.ExerciseQueryBuilder;
using Web.Code.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Web.Data.QueryBuilder;

public class ExerciseQueryer
{
    [DebuggerDisplay("{Exercise}: {Variation}")]
    public record QueryResults(
        User? User,
        Exercise Exercise,
        Variation Variation,
        ExerciseVariation ExerciseVariation,
        UserExercise? UserExercise,
        UserExerciseVariation? UserExerciseVariation,
        UserVariation? UserVariation,
        Tuple<Variation, string?>? EasierVariation,
        Tuple<Variation, string?>? HarderVariation
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
        public bool AllCurrentVariationsIgnored { get; init; }
        public Tuple<Variation, string?>? HarderVariation { get; init; }
        public Tuple<Variation, string?>? EasierVariation { get; init; }
        public bool IsMinProgressionInRange { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
        public int? NextProgression { get; init; }
    }

    private class ExerciseComparer : IEqualityComparer<InProgressQueryResults>
    {
        public bool Equals(InProgressQueryResults? x, InProgressQueryResults? y)
        {
            return x?.Exercise.Id == y?.Exercise.Id;
        }

        public int GetHashCode([DisallowNull] InProgressQueryResults obj)
        {
            return HashCode.Combine(obj.Exercise.Id);
        }
    }

    public readonly CoreContext Context;
    public readonly bool IgnoreGlobalQueryFilters = false;

    public required User? User;

    public required ExerciseType? ExerciseType;
    public required MuscleGroups? RecoveryMuscle;
    public required MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    public required MuscleContractions? MuscleContractions;
    public required MuscleMovement? MuscleMovement;
    public required OrderByEnum OrderBy = OrderByEnum.None;
    public required SportsFocus? SportsFocus;
    public required int SkipCount = 0;
    public required bool? Unilateral = null;
    public required bool? AntiGravity = null;
    public required IEnumerable<int>? EquipmentIds;

    public required ExclusionOptions ExclusionOptions { get; init; }
    public required BonusOptions BonusOptions { get; init; }
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
                        UserExercise = r.PrerequisiteExercise.UserExercises.FirstOrDefault(up => up.User == User),
                        UserVariations = r.PrerequisiteExercise.ExerciseVariations.Select(ev => ev.Variation.UserVariations.FirstOrDefault(ev => ev.User == User))
                    })
                    // Require the prerequisites show first
                    .All(p => User == null || (p.UserExercise != null && (
                        // The prerequisite exercise was ignored
                        p.UserExercise.Ignore
                        // All of the exercise's variations were ignroed
                        || p.UserVariations.All(uv => uv.Ignore)
                        // User is at or past the required proficiency level
                        || p.UserExercise.Progression >= p.Proficiency)
                    ))
            );

        var allExerciseVariationsQuery = Context.ExerciseVariations
            .Join(eligibleExercisesQuery, o => o.Exercise.Id, i => i.Exercise.Id, (o, i) => new
            {
                ExerciseVariation = o,
                o.Variation,
                i.Exercise,
                i.UserExercise
            })
            .Select(a => new
            {
                a.ExerciseVariation,
                // Out of range when the exercise is too difficult for the user
                IsMinProgressionInRange = User == null
                    // This exercise variation has no minimum 
                    || a.ExerciseVariation.Progression.Min == null
                    // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                    // Make sure the first variation we show in an exercise progression is a beginner variation.
                    || (a.UserExercise == null && a.ExerciseVariation.Progression.Min == null)
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise != null
                        // If we want to cap at the exercise's proficiency level 
                        && (!Proficiency.DoCapAtProficiency || a.ExerciseVariation.Progression.Min <= a.ExerciseVariation.Exercise.Proficiency)
                        // Check against the user's progression level, taking into account the proficiency adjustment
                        && a.ExerciseVariation.Progression.Min <= (a.UserExercise.Progression * (Proficiency.CapAtUsersProficiencyPercent != null ? Proficiency.CapAtUsersProficiencyPercent : 1))
                    ),
                // Out of range when the exercise is too easy for the user
                IsMaxProgressionInRange = User == null
                    // This exercise variation has no maximum
                    || a.ExerciseVariation.Progression.Max == null
                    // User hasn't ever seen this exercise before. Show it so an ExerciseUserExercise record is made.
                    || a.UserExercise == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise != null && a.UserExercise!.Progression < a.ExerciseVariation.Progression.Max),
            });

        var baseQuery = Context.Variations
            .AsNoTracking() // Don't update any entity
            .Include(i => i.Intensities)
            .Include(i => i.DefaultInstruction)
            // If OnlyWeights is false, filter down the included equipment groups to only those not using any weight
            .Include(i => i.Instructions.Where(eg => eg.Parent == null).Where(eg => WeightOptions.OnlyWeights != false || !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight) || eg.Link != null)))
                // To display the equipment required for the exercise in the newsletter
                .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Include(i => i.Instructions.Where(eg => eg.Parent == null).Where(eg => WeightOptions.OnlyWeights != false || !eg.IsWeight && (!eg.Children.Any() || eg.Children.Any(c => !c.IsWeight) || eg.Link != null)))
                .ThenInclude(eg => eg.Children)
                    // To display the equipment required for the exercise in the newsletter
                    .ThenInclude(eg => eg.Equipment.Where(e => e.DisabledReason == null))
            .Join(allExerciseVariationsQuery, o => o.Id, i => i.ExerciseVariation.Variation.Id, (o, i) => new
            {
                Variation = o,
                i.ExerciseVariation,
                i.IsMinProgressionInRange,
                i.IsMaxProgressionInRange
            })
            .Join(eligibleExercisesQuery, o => o.ExerciseVariation.Exercise.Id, i => i.Exercise.Id, (o, i) => new
            {
                o.Variation,
                o.ExerciseVariation,
                o.IsMinProgressionInRange,
                o.IsMaxProgressionInRange,
                i.Exercise,
                i.UserExercise,
                AllCurrentVariationsIgnored = allExerciseVariationsQuery
                    .Where(ev => ev.ExerciseVariation.ExerciseId == i.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .Select(ev => ev.ExerciseVariation.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)).All(uv => uv.Ignore)
            })
            // Don't grab exercises that we want to ignore.
            .Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id))
            // Don't grab variations that we want to ignore.
            .Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id))
            .Select(a => new InProgressQueryResults()
            {
                UserExercise = a.UserExercise,
                UserVariation = a.Variation.UserVariations.FirstOrDefault(uv => uv.User == User),
                UserExerciseVariation = a.ExerciseVariation.UserExerciseVariations.FirstOrDefault(uv => uv.User == User),
                Exercise = a.Exercise,
                Variation = a.Variation,
                ExerciseVariation = a.ExerciseVariation,
                EasierVariation = Tuple.Create(Context.ExerciseVariations
                    .Where(ev => ev.ExerciseId == a.Exercise.Id)
                    // Don't show ignored variations? (untested)
                    //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                    .OrderByDescending(ev => ev.Progression.Max)
                    // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                    .ThenBy(ev => a.AllCurrentVariationsIgnored ? ev.Variation.UserVariations.First(uv => uv.User == User).Ignore : !ev.Variation.UserVariations.First(uv => uv.User == User).Ignore)
                    .First(ev => ev != a.ExerciseVariation 
                        && (
                            // Current progression is in range, choose the previous progression by looking at the user's current progression level
                            (a.IsMinProgressionInRange && a.IsMaxProgressionInRange && ev.Progression.Max != null && ev.Progression.Max <= (a.UserExercise == null ? UserExercise.MinUserProgression : a.UserExercise.Progression))
                            // Current progression is out of range, choose the previous progression by looking at current exercise's min progression
                            || (!a.IsMinProgressionInRange && ev.Progression.Max != null && ev.Progression.Max <= a.ExerciseVariation.Progression.Min)
                        ))
                    .Variation, !a.IsMinProgressionInRange ? (a.AllCurrentVariationsIgnored ? "Ignored" : "Missing Equipment") : null),
                HarderVariation = Tuple.Create(Context.ExerciseVariations
                    .Where(ev => ev.ExerciseId == a.Exercise.Id)
                    // Don't show ignored variations? (untested)
                    //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                    .OrderBy(ev => ev.Progression.Min)
                    // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                    .ThenBy(ev => a.AllCurrentVariationsIgnored ? ev.Variation.UserVariations.First(uv => uv.User == User).Ignore : !ev.Variation.UserVariations.First(uv => uv.User == User).Ignore)
                    .First(ev => ev != a.ExerciseVariation
                        && (
                            // Current progression is in range, choose the next progression by looking at the user's current progression level
                            (a.IsMinProgressionInRange && a.IsMaxProgressionInRange && ev.Progression.Min != null && ev.Progression.Min > (a.UserExercise == null ? UserExercise.MinUserProgression : a.UserExercise.Progression))
                            // Current progression is out of range, choose the next progression by looking at current exercise's min progression
                            || (!a.IsMaxProgressionInRange && ev.Progression.Min != null && ev.Progression.Min > a.ExerciseVariation.Progression.Max)
                        ))
                    .Variation, !a.IsMaxProgressionInRange ? (a.AllCurrentVariationsIgnored ? "Ignored" : "Missing Equipment") : null),
                IsMinProgressionInRange = a.IsMinProgressionInRange,
                IsMaxProgressionInRange = a.IsMaxProgressionInRange,
                NextProgression = a.UserExercise == null ? null : Context.ExerciseVariations
                    // Stop at the lower bounds of variations
                    .Where(ev => ev.ExerciseId == a.Exercise.Id)
                    // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                    .Where(ev => !ev.Variation.UserVariations.First(uv => uv.User == User).Ignore)
                    .Select(ev => ev.Progression.Min)
                    // Stop at the upper bounds of variations
                    .Union(Context.ExerciseVariations
                        .Where(ev => ev.ExerciseId == a.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => !ev.Variation.UserVariations.First(uv => uv.User == User).Ignore)
                        .Select(ev => ev.Progression.Max)
                    )
                    .Where(mp => mp.HasValue && mp > a.UserExercise.Progression)
                    .OrderBy(mp => mp - a.UserExercise.Progression)
                    .First(),
                // Grab variations that are in the user's progression range. Skip filtering on these so we can see if we need to grab an out-of-range progression.
                AllCurrentVariationsIgnored = a.AllCurrentVariationsIgnored,
            })
            // Don't grab variations that the user wants to ignore.
            .Where(i => i.UserVariation == null || !i.UserVariation.Ignore);

        if (IgnoreGlobalQueryFilters)
        {
            baseQuery = baseQuery.IgnoreQueryFilters();
        }

        if (User != null)
        {
            baseQuery = baseQuery.Where(i =>
                            // User owns at least one equipment in at least one of the optional equipment groups
                            i.Variation.Instructions.Any(eg => !eg.Equipment.Any())
                            || i.Variation.Instructions.Where(eg => eg.Equipment.Any()).Any(peg =>
                                peg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id))
                                && (
                                    !peg.Children.Any()
                                    || peg.Link != null // Exercise can be done without child equipment
                                    || peg.Children.Any(ceg => ceg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                                )
                            )
                        );
        }

        baseQuery = Filters.FilterMovementPattern(baseQuery, MovementPattern.MovementPatterns);
        baseQuery = Filters.FilterMuscleGroup(baseQuery, MuscleGroup.MuscleGroups, include: true, MuscleGroup.MuscleTarget);
        baseQuery = Filters.FilterMuscleGroup(baseQuery, MuscleGroup.ExcludeRecoveryMuscle, include: false, v => v.Variation.StrengthMuscles | v.Variation.StabilityMuscles);
        baseQuery = Filters.FilterEquipmentIds(baseQuery, EquipmentIds);
        baseQuery = Filters.FilterRecoveryMuscle(baseQuery, RecoveryMuscle);
        baseQuery = Filters.FilterSportsFocus(baseQuery, SportsFocus);
        baseQuery = Filters.FilterIncludeBonus(baseQuery, BonusOptions.Bonus, BonusOptions.OnlyBonus);
        baseQuery = Filters.FilterAntiGravity(baseQuery, AntiGravity);
        baseQuery = Filters.FilterMuscleContractions(baseQuery, MuscleContractions);
        baseQuery = Filters.FilterMuscleMovement(baseQuery, MuscleMovement);
        baseQuery = Filters.FilterExerciseType(baseQuery, ExerciseType);
        baseQuery = Filters.FilterIsUnilateral(baseQuery, Unilateral);
        baseQuery = Filters.FilterOnlyWeights(baseQuery, WeightOptions.OnlyWeights);        

        var queryResults = (await baseQuery.ToListAsync()).AsEnumerable();

        if (User != null)
        {
            // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
            queryResults = queryResults.GroupBy(i => i, new ExerciseComparer())
                                .SelectMany(g =>
                                    // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                    g.Where(a => a.IsMinProgressionInRange && a.IsMaxProgressionInRange).NullIfEmpty()
                                        ?? g.Where(a => !a.IsMaxProgressionInRange && Proficiency.AllowLesserProgressions)
                                            // Only grab lower progressions when all of the current variations are ignored 
                                            //.Where(a => a.AllCurrentVariationsIgnored) // Not checking this because it's possible a lack of equipment causes the current variation to not show.
                                            // FIXED: If two variations have the same max proficiency, should we select both? Yes
                                            .GroupBy(e => e.ExerciseVariation.Progression.GetMaxOrDefault).OrderByDescending(k => k.Key).Take(1).SelectMany(k => k).NullIfEmpty()
                                        // If there is no lesser progression, select the next higher variation.
                                        // We do this so the user doesn't get stuck at the beginning of an exercise track if they ignore the first variation instead of progressing.
                                        ?? g.Where(a => !a.IsMinProgressionInRange /*&& Proficiency.AllowGreaterProgressions*/)
                                            // Only grab higher progressions when all of the current variations are ignored
                                            .Where(a => a.AllCurrentVariationsIgnored)
                                            // FIXED: When filtering down to something like MovementPatterns,
                                            // ...if the next highest variation that passes the MovementPattern filter is higher than the next highest variation that doesn't,
                                            // ...then we will get a twice-as-difficult next variation.
                                            .Where(a => a.ExerciseVariation.Progression.GetMinOrDefault <= (g.Key.NextProgression ?? UserExercise.MaxUserProgression))
                                            // FIXED: If two variations have the same min proficiency, should we select both? Yes
                                            .GroupBy(e => e.ExerciseVariation.Progression.GetMinOrDefault).OrderBy(k => k.Key).Take(1).SelectMany(k => k)
                                );
        }

        // OrderBy must come after query or you get duplicates.
        // No longer ordering by weighted exercises, since that was to prioritize free weights over advanced calisthenics.
        // Now all advanced calisthenics shoulsd be bonus exercises.
        var orderedResults = queryResults
            // Show exercises that the user has rarely seen
            .OrderBy(a => a.UserExercise == null ? DateOnly.MinValue : a.UserExercise.LastSeen)
            // Show variations that the user has rarely seen
            .ThenBy(a => a.UserExerciseVariation == null ? DateOnly.MinValue : a.UserExerciseVariation.LastSeen)
            // Mostly for the demo, show mostly random exercises
            .ThenBy(a => Guid.NewGuid());

        var muscleTarget = MuscleGroup.MuscleTarget.Compile();
        var finalResults = new List<QueryResults>();
        if (OrderBy == OrderByEnum.UniqueMuscles)
        {
            // Yikes
            for (var i = 0; i < orderedResults.Count(); i++)
            {
                var musclesWorkedSoFar = finalResults.WorkedMuscles(addition: MusclesAlreadyWorked, muscleTarget: muscleTarget);
                var stack = orderedResults
                    // The variation works at least 1 unworked muscles. `Where` preserves order.
                    .Where(vm => BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(vm).UnsetFlag32(musclesWorkedSoFar))) <= BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups) - 1)
                    // Order by how many unique primary muscles the exercise works. After the least seen exercises, choose the optimal routine
                    .OrderBy(vm => /*least seen:*/ i < SkipCount ? 0 : BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(vm).UnsetFlag32(musclesWorkedSoFar))))
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
        else if (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null)
        {
            // Double Yikes
            while (MuscleGroup.AtLeastXUniqueMusclesPerExercise > 1)
            {
                foreach (var exercise in orderedResults)
                {
                    if (finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                    {
                        continue;
                    }

                    var musclesWorkedSoFar = finalResults.WorkedMuscles(addition: MusclesAlreadyWorked, muscleTarget: muscleTarget);
                    // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                    if (BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups.UnsetFlag32(muscleTarget(exercise).UnsetFlag32(musclesWorkedSoFar))) <= BitOperations.PopCount((ulong)MuscleGroup.MuscleGroups) - MuscleGroup.AtLeastXUniqueMusclesPerExercise)
                    {
                        finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
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

                var musclesWorkedSoFar = finalResults.WorkedMuscles(addition: MusclesAlreadyWorked, muscleTarget: muscleTarget);
                // Grab any muscle groups we missed in the previous loops. Include isolation exercises here
                if (MuscleGroup.AtLeastXUniqueMusclesPerExercise == 0 || muscleTarget(exercise).UnsetFlag32(musclesWorkedSoFar).HasAnyFlag32(MuscleGroup.MuscleGroups))
                {
                    finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                }
            }
        }
        else if (MovementPattern.MovementPatterns.HasValue && MovementPattern.IsUnique)
        {
            var values = Enum.GetValues<MovementPattern>().Where(v => MovementPattern.MovementPatterns.Value.HasFlag(v));
            foreach (var movementPattern in values)
            {
                foreach (var exercise in orderedResults)
                {
                    // Choose either compound exercises that cover at least X muscles in the targeted muscles set
                    if (exercise.Variation.MovementPattern.HasAnyFlag32(movementPattern))
                    {
                        finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
                        break;
                    }
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
