using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data.Query.Options;
using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Data.Query;

public class QueryRunner
{
    [DebuggerDisplay("{Exercise}")]
    private class ExercisesQueryResults
    {
        public Exercise Exercise { get; init; } = null!;
        public UserExercise UserExercise { get; init; } = null!;
    }

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
        public bool IsMinProgressionInRange { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
        public bool AllCurrentVariationsIgnored { get; set; }
        public bool AllCurrentVariationsMissingEquipment { get; set; }
        public Tuple<Variation?, string?>? HarderVariation { get; set; }
        public Tuple<Variation?, string?>? EasierVariation { get; set; }
        public int? NextProgression { get; set; }
    }

    [DebuggerDisplay("{Variation}")]
    private class VariationsQueryResults
    {
        public Variation Variation { get; init; } = null!;
        public UserVariation UserVariation { get; init; } = null!;
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    private class ExerciseVariationsQueryResults
        : IExerciseVariationCombo
    {
        public Exercise Exercise { get; init; } = null!;
        public Variation Variation { get; init; } = null!;
        public ExerciseVariation ExerciseVariation { get; init; } = null!;
        public UserExercise UserExercise { get; init; } = null!;
        public UserExerciseVariation UserExerciseVariation { get; init; } = null!;
        public UserVariation UserVariation { get; init; } = null!;
        public bool IsMinProgressionInRange { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
        public bool UserOwnsEquipment { get; init; }
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

    public required ExclusionOptions ExclusionOptions { get; init; }
    public required ProficiencyOptions Proficiency { get; init; }
    public required MovementPatternOptions MovementPattern { get; init; }
    public required MuscleGroupOptions MuscleGroup { get; init; }
    public required WeightOptions WeightOptions { get; init; }

    // TODO: Move these into options classes
    public required ExerciseType? ExerciseType;
    public required MuscleGroups? RecoveryMuscle;
    public required MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    public required MuscleContractions? MuscleContractions;
    public required MuscleMovement? MuscleMovement;
    public required OrderBy OrderBy = OrderBy.None;
    public required SportsFocus? SportsFocus;
    public required int SkipCount = 0;
    public required bool? Unilateral = null;
    public required bool? AntiGravity = null;
    public required IEnumerable<int>? EquipmentIds;

    public QueryRunner(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }

    /// <summary>
    /// Queries the db for the data
    /// </summary>
    public async Task<IList<QueryResults>> Query()
    {
        var exercisesQuery = Context.Exercises.AsNoTracking()
            .Include(e => e.Prerequisites) // TODO Only necessary for the /exercises list, not the newsletter
                .ThenInclude(p => p.PrerequisiteExercise)
            .Select(i => new ExercisesQueryResults()
            {
                Exercise = i,
                UserExercise = i.UserExercises.First(ue => ue.User == User)
            });

        var variationsQuery = Context.Variations.AsNoTracking()
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
            .Select(v => new VariationsQueryResults()
            {
                Variation = v,
                UserVariation = v.UserVariations.First(uv => uv.User == User)
            });

        var exerciseVariationsQuery = Context.ExerciseVariations.AsNoTracking()
            .Join(exercisesQuery, o => o.ExerciseId, i => i.Exercise.Id, (o, i) => new
            {
                ExerciseVariation = o,
                i.Exercise,
                i.UserExercise
            })
            .Join(variationsQuery, o => o.ExerciseVariation.VariationId, i => i.Variation.Id, (o, i) => new
            {
                o.ExerciseVariation,
                o.Exercise,
                o.UserExercise,
                i.Variation,
                i.UserVariation
            })
            .Select(a => new ExerciseVariationsQueryResults()
            {
                ExerciseVariation = a.ExerciseVariation,
                Exercise = a.Exercise,
                Variation = a.Variation,
                UserExercise = a.UserExercise,
                UserVariation = a.UserVariation,
                UserExerciseVariation = a.ExerciseVariation.UserExerciseVariations.First(uev => uev.User == User),
                //UserExercise = a.ExerciseVariation.Variation.UserExercises.First(uev => uev.User == User),
                // Out of range when the exercise is too difficult for the user
                IsMinProgressionInRange = User == null
                    // This exercise variation has no minimum 
                    || a.ExerciseVariation.Progression.Min == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || a.ExerciseVariation.Progression.Min <= Math.Min(a.UserExercise.Progression, Proficiency.DoCapAtProficiency ? a.ExerciseVariation.Exercise.Proficiency : UserExercise.MaxUserProgression),
                // Out of range when the exercise is too easy for the user
                IsMaxProgressionInRange = User == null
                    // This exercise variation has no maximum
                    || a.ExerciseVariation.Progression.Max == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise.Progression < a.ExerciseVariation.Progression.Max),
                // User owns at least one equipment in at least one of the optional equipment groups
                UserOwnsEquipment = User == null
                    || a.Variation.Instructions.Any(eg => !eg.Equipment.Any())
                    || a.Variation.Instructions.Where(eg => eg.Equipment.Any()).Any(peg =>
                        peg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id))
                        && (
                            !peg.Children.Any()
                            || peg.Link != null
                            // Exercise can be done without child equipment
                            || peg.Children.Any(ceg => ceg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                        )
                    )
            });

        var filteredQuery = exerciseVariationsQuery
            // Don't grab exercises that the user wants to ignore
            .Where(i => i.UserExercise.Ignore != true)
            // Filter down to variations the user owns equipment for
            .Where(i => i.UserOwnsEquipment)
            // Don't grab exercises that we want to ignore
            .Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id))
            // Only show these exercises if the user has completed the previous reqs
            .Where(i => i.Exercise.Prerequisites
                    .Select(r => new
                    {
                        r.PrerequisiteExercise.Proficiency,
                        UserExercise = r.PrerequisiteExercise.UserExercises.First(up => up.User == User),
                        UserVariations = r.PrerequisiteExercise.ExerciseVariations.Select(ev => ev.Variation.UserVariations.First(ev => ev.User == User))
                    })
                    // Require the prerequisites show first
                    .All(p => User == null
                        // The prerequisite exercise was ignored
                        || p.UserExercise.Ignore
                        // All of the exercise's variations were ignroed
                        || p.UserVariations.All(uv => uv.Ignore)
                        // User is past the required proficiency level.
                        // Not checking 'at' because the proficiency is used as the starting progression level for a user,
                        // ... and we don't want to show handstand pushups before the user has seen and progressed pushups.
                        || p.UserExercise.Progression > p.Proficiency
                    )
            )
            // Don't grab variations that the user wants to ignore.
            .Where(i => i.UserVariation.Ignore != true)
            // Don't grab variations that we want to ignore.
            .Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));

        filteredQuery = Filters.FilterExerciseType(filteredQuery, ExerciseType);
        filteredQuery = Filters.FilterRecoveryMuscle(filteredQuery, RecoveryMuscle);
        filteredQuery = Filters.FilterSportsFocus(filteredQuery, SportsFocus);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPattern.MovementPatterns);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.MuscleGroups, include: true, MuscleGroup.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.ExcludeRecoveryMuscle, include: false, v => v.Variation.StrengthMuscles | v.Variation.StabilityMuscles);
        filteredQuery = Filters.FilterEquipmentIds(filteredQuery, EquipmentIds);
        filteredQuery = Filters.FilterAntiGravity(filteredQuery, AntiGravity);
        filteredQuery = Filters.FilterMuscleContractions(filteredQuery, MuscleContractions);
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovement);
        filteredQuery = Filters.FilterIsUnilateral(filteredQuery, Unilateral);
        filteredQuery = Filters.FilterOnlyWeights(filteredQuery, WeightOptions.OnlyWeights);

        if (IgnoreGlobalQueryFilters)
        {
            filteredQuery = filteredQuery.IgnoreQueryFilters();
        }

        var queryResults = (await filteredQuery.AsNoTracking().TagWithCallSite()
            /*.Select(i =>  new
            {
                i.ExerciseVariation,
                i.Exercise,
                i.Variation,
                i.UserVariation,
                i.UserExercise,
                i.UserExerciseVariation,
                i.IsMinProgressionInRange,
                i.IsMaxProgressionInRange,
                // Grab variations that are in the user's progression range. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                AllCurrentVariationsIgnored = exerciseVariationsQuery
                    .Where(ev => ev.Exercise.Id == i.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => ev.UserVariation.Ignore),
                // Grab variations that the user owns the necessary equipment for. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                AllCurrentVariationsMissingEquipment = exerciseVariationsQuery
                    .Where(ev => ev.Exercise.Id == i.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => !ev.UserOwnsEquipment)
            })*/
            .Select(a => new InProgressQueryResults()
            {
                UserExercise = a.UserExercise,
                UserVariation = a.UserVariation,
                UserExerciseVariation = a.UserExerciseVariation,
                Exercise = a.Exercise,
                Variation = a.Variation,
                ExerciseVariation = a.ExerciseVariation,
                IsMinProgressionInRange = a.IsMinProgressionInRange,
                IsMaxProgressionInRange = a.IsMaxProgressionInRange,
                //AllCurrentVariationsIgnored = a.AllCurrentVariationsIgnored,
                //AllCurrentVariationsIgnored = a.AllCurrentVariationsIgnored,
            }).ToListAsync()).AsEnumerable();

        if (User != null)
        {
            var allVariations = await Context.ExerciseVariations.Include(ev => ev.Variation).ThenInclude(v => v.UserVariations).ToListAsync();
            var exerciseVariationsQueryResults = await exerciseVariationsQuery.ToListAsync();
            foreach (var queryResult in queryResults)
            {
                // Grab variations that are in the user's progression range. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsIgnored = exerciseVariationsQueryResults
                    .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => ev.UserVariation.Ignore);

                // Grab variations that the user owns the necessary equipment for. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsMissingEquipment = exerciseVariationsQueryResults
                    .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => !ev.UserOwnsEquipment);

                queryResult.EasierVariation = Tuple.Create(allVariations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderByDescending(ev => ev.Progression.Max)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == true : ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == false)
                        .FirstOrDefault(ev => ev != queryResult.ExerciseVariation
                            && (
                                // Current progression is in range, choose the previous progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.Progression.Max != null && ev.Progression.Max <= (queryResult.UserExercise == null ? UserExercise.MinUserProgression : queryResult.UserExercise.Progression))
                                // Current progression is out of range, choose the previous progression by looking at current exercise's min progression
                                || (!queryResult.IsMinProgressionInRange && ev.Progression.Max != null && ev.Progression.Max <= queryResult.ExerciseVariation.Progression.Min)
                            ))?
                        .Variation, !queryResult.IsMinProgressionInRange ? (queryResult.AllCurrentVariationsIgnored ? "Ignored" : "Missing Equipment") : null);

                queryResult.HarderVariation = Tuple.Create(allVariations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderBy(ev => ev.Progression.Min)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == true : ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == false)
                        .FirstOrDefault(ev => ev != queryResult.ExerciseVariation
                            && (
                                // Current progression is in range, choose the next progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.Progression.Min != null && ev.Progression.Min > (queryResult.UserExercise == null ? UserExercise.MinUserProgression : queryResult.UserExercise.Progression))
                                // Current progression is out of range, choose the next progression by looking at current exercise's min progression
                                || (!queryResult.IsMaxProgressionInRange && ev.Progression.Min != null && ev.Progression.Min > queryResult.ExerciseVariation.Progression.Max)
                            ))?
                        .Variation, !queryResult.IsMaxProgressionInRange ? (queryResult.AllCurrentVariationsIgnored ? "Ignored" : "Missing Equipment") : null);

                queryResult.NextProgression = queryResult.UserExercise == null ? null : allVariations
                        // Stop at the lower bounds of variations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == false)
                        .Select(ev => ev.Progression.Min)
                        // Stop at the upper bounds of variations
                        .Union(allVariations
                            .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                            // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                            .Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)?.Ignore == false)
                            .Select(ev => ev.Progression.Max)
                        )
                        .Where(mp => mp.HasValue && mp > queryResult.UserExercise.Progression)
                        .OrderBy(mp => mp - queryResult.UserExercise.Progression)
                        .FirstOrDefault();
            }

            // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
            queryResults = queryResults.GroupBy(i => i, new ExerciseComparer())
                                .SelectMany(g =>
                                    // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                    g.Where(a => a.IsMinProgressionInRange && a.IsMaxProgressionInRange).NullIfEmpty()
                                        ?? g.Where(a => !a.IsMaxProgressionInRange /*&& Proficiency.AllowLesserProgressions*/)
                                            // Only grab lower progressions when all of the current variations are ignored.
                                            // It's possible a lack of equipment causes the current variation to not show.
                                            .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
                                            // FIXED: If two variations have the same max proficiency, should we select both? Yes
                                            .GroupBy(e => e.ExerciseVariation.Progression.GetMaxOrDefault).OrderByDescending(k => k.Key).Take(1).SelectMany(k => k).NullIfEmpty()
                                        // If there is no lesser progression, select the next higher variation.
                                        // We do this so the user doesn't get stuck at the beginning of an exercise track if they ignore the first variation instead of progressing.
                                        ?? g.Where(a => !a.IsMinProgressionInRange /*&& Proficiency.AllowGreaterProgressions*/)
                                            // Only grab higher progressions when all of the current variations are ignored.
                                            // It's possible a lack of equipment causes the current variation to not show.
                                            .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
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
        if (OrderBy == OrderBy.UniqueMuscles)
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
            OrderBy.None => finalResults,
            OrderBy.UniqueMuscles => finalResults,
            OrderBy.Progression => finalResults.Take(SkipCount).Concat(finalResults.Skip(SkipCount)
                                                   .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max))
                                                   .ToList(),
            OrderBy.MuscleTarget => finalResults.Take(SkipCount).Concat(finalResults.Skip(SkipCount)
                                                    .OrderByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)) - BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups)))
                                                    .ThenBy(vm => BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups))))
                                                    .ToList(),
            OrderBy.Name => finalResults.OrderBy(vm => vm.Exercise.Name)
                                            .ThenBy(vm => vm.Variation.Name)
                                            .ToList(),
            _ => finalResults,
        };
    }
}
