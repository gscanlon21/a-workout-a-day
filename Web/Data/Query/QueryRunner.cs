using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data.Query.Options;
using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;

namespace Web.Data.Query;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
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

    [DebuggerDisplay("{ExerciseId}")]
    private class PrerequisitesQueryResults
    {
        public int ExerciseId { get; init; }
        public int UserExerciseProgression { get; init; }
        public int ExerciseProficiency { get; init; }
        public DateOnly UserExerciseLastSeen { get; init; }
        public Progression ExerciseVariationProgression { get; init; } = null!;
        public DateOnly UserExerciseVariationLastSeen { get; init; }
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

    public required SelectionOptions SelectionOptions { get; init; }
    public required ExclusionOptions ExclusionOptions { get; init; }
    public required ExerciseOptions ExerciseOptions { get; init; }
    public required ProficiencyOptions Proficiency { get; init; }
    public required MovementPatternOptions MovementPattern { get; init; }
    public required MuscleGroupOptions MuscleGroup { get; init; }
    public required WeightOptions WeightOptions { get; init; }
    public required ExerciseTypeOptions ExerciseTypeOptions { get; init; }
    public required OrderByOptions OrderByOptions { get; init; }
    public required JointsOptions JointsOptions { get; init; }
    public required SportsOptions SportsOptions { get; init; }
    public required EquipmentOptions EquipmentOptions { get; init; }
    public required ExerciseFocusOptions ExerciseFocusOptions { get; init; }
    public required MuscleContractionsOptions MuscleContractionsOptions { get; init; }
    public required MuscleMovementOptions MuscleMovementOptions { get; init; }

    public QueryRunner(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }

    private IQueryable<ExercisesQueryResults> CreateExercisesQuery(bool includePrerequisites)
    {
        var query = Context.Exercises.TagWith(nameof(CreateExercisesQuery));

        if (includePrerequisites)
        {
            query = query
                .Include(e => e.Prerequisites)
                    .ThenInclude(p => p.PrerequisiteExercise);
        }

        return query.Select(i => new ExercisesQueryResults()
        {
            Exercise = i,
            UserExercise = i.UserExercises.First(ue => ue.User == User)
        });
    }

    private IQueryable<VariationsQueryResults> CreateVariationsQuery(bool includeIntensities, bool includeInstructions)
    {
        var query = Context.Variations.AsNoTracking().TagWith(nameof(CreateVariationsQuery));

        if (includeIntensities)
        {
            query = query.Include(i => i.Intensities);
        }

        if (includeInstructions)
        {
            query = query
                .Include(i => i.DefaultInstruction)
                // Instruction equipment is auto included
                .Include(i => i.Instructions.Where(eg => eg.Parent == null && eg.Equipment.Any()))
                    .ThenInclude(eg => eg.Children.Where(ceg => ceg.Equipment.Any()));
        }

        return query.Select(v => new VariationsQueryResults()
        {
            Variation = v,
            UserVariation = v.UserVariations.First(uv => uv.User == User)
        });
    }

    private IQueryable<ExerciseVariationsQueryResults> CreateExerciseVariationsQuery(bool includeIntensities, bool includeInstructions, bool includePrerequisites)
    {
        return Context.ExerciseVariations.AsNoTracking().TagWith(nameof(CreateExerciseVariationsQuery))
            .Join(CreateExercisesQuery(includePrerequisites: includePrerequisites), 
                o => o.ExerciseId, i => i.Exercise.Id, (o, i) => new
            {
                ExerciseVariation = o,
                i.Exercise,
                i.UserExercise
            })
            .Join(CreateVariationsQuery(includeIntensities: includeIntensities, includeInstructions: includeInstructions), 
                o => o.ExerciseVariation.VariationId, i => i.Variation.Id, (o, i) => new
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
                    || a.ExerciseVariation.Progression.Min <= (Proficiency.DoCapAtProficiency ? Math.Min(a.ExerciseVariation.Exercise.Proficiency, a.UserExercise.Progression) : a.UserExercise.Progression),
                // Out of range when the exercise is too easy for the user
                IsMaxProgressionInRange = User == null
                    // This exercise variation has no maximum
                    || a.ExerciseVariation.Progression.Max == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise.Progression < a.ExerciseVariation.Progression.Max),
                // User owns at least one equipment in at least one of the optional equipment groups
                UserOwnsEquipment = User == null
                    // There is an instruction that does not require any equipment
                    || a.Variation.Instructions.Any(eg => !eg.Equipment.Any())
                    // Out of the instructions that require equipment, the user owns the equipment for the root instruction and the root instruction can be done on its own, or the user own the equipment of the child instructions. 
                    || a.Variation.Instructions.Where(eg => eg.Equipment.Any()).Any(peg =>
                        // User owns equipment for the root instruction 
                        peg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id))
                        && (
                            // Root instruction can be done on its own
                            peg.Link != null
                            // Or the user owns the equipment for the child instructions
                            || peg.Children.Any(ceg => ceg.Equipment.Any(e => User.EquipmentIds.Contains(e.Id)))
                        )
                    )
            });
    }

    private IQueryable<ExerciseVariationsQueryResults> CreateFilteredExerciseVariationsQuery(bool includeIntensities, bool includeInstructions, bool includePrerequisites)
    {
        var filteredQuery = CreateExerciseVariationsQuery(
                includeIntensities: includeIntensities, 
                includeInstructions: includeInstructions, 
                includePrerequisites: includePrerequisites)
            .AsNoTracking().TagWith(nameof(CreateFilteredExerciseVariationsQuery))
            // Filter down to variations the user owns equipment for
            .Where(vm => vm.UserOwnsEquipment)
            // Don't grab exercises that the user wants to ignore
            .Where(vm => vm.UserExercise.Ignore != true)
            // Don't grab variations that the user wants to ignore
            .Where(vm => vm.UserVariation.Ignore != true)
            // Don't grab groups that we want to ignore
            .Where(vm => (ExclusionOptions.ExerciseGroups & vm.Exercise.Groups) == 0)
            // Don't grab exercises that we want to ignore
            .Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id))
            // Don't grab exercises that we want to ignore
            .Where(vm => !ExclusionOptions.ExerciseVariationIds.Contains(vm.ExerciseVariation.Id))
            // Don't grab variations that we want to ignore.
            .Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));

        if (IgnoreGlobalQueryFilters)
        {
            filteredQuery = filteredQuery.IgnoreQueryFilters();
        }

        return filteredQuery;
    }

    /// <summary>
    /// Queries the db for the data
    /// </summary>
    public async Task<IList<QueryResults>> Query()
    {
        var filteredQuery = CreateFilteredExerciseVariationsQuery(includeIntensities: true, includeInstructions: true, includePrerequisites: true);

        filteredQuery = Filters.FilterExerciseType(filteredQuery, ExerciseTypeOptions.ExerciseType);
        filteredQuery = Filters.FilterJoints(filteredQuery, JointsOptions.Joints);
        filteredQuery = Filters.FilterExercises(filteredQuery, ExerciseOptions.ExerciseIds);
        filteredQuery = Filters.FilterExerciseVariations(filteredQuery, ExerciseOptions.ExerciseVariationIds);
        filteredQuery = Filters.FilterVariations(filteredQuery, ExerciseOptions.VariationIds);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExerciseFocus);
        filteredQuery = Filters.FilterSportsFocus(filteredQuery, SportsOptions.SportsFocus);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPattern.MovementPatterns);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.MuscleGroups, include: true, MuscleGroup.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.ExcludeRecoveryMuscle, include: false, v => v.Variation.StrengthMuscles | v.Variation.SecondaryMuscles);
        filteredQuery = Filters.FilterEquipmentIds(filteredQuery, EquipmentOptions.EquipmentIds);
        filteredQuery = Filters.FilterMuscleContractions(filteredQuery, MuscleContractionsOptions.MuscleContractions);
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);
        filteredQuery = Filters.FilterOnlyWeights(filteredQuery, WeightOptions.OnlyWeights);

        var queryResults = await filteredQuery.AsNoTracking().TagWithCallSite()
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
            }).ToListAsync();

        var filteredResults = new List<InProgressQueryResults>();
        if (User == null)
        {
            filteredResults = queryResults;
        }
        else
        {
            // Grab a half-filtered list of exercises to check the prerequisites from.
            // We don't want to see a rehab exercise as a prerequisite when strength training.
            // We do want to see Planks (isometric) and Dynamic Planks (isotonic) as a prereq for Mountain Climbers (plyo).
            var checkPrerequisitesFromQuery = CreateFilteredExerciseVariationsQuery(includeIntensities: false, includeInstructions: false, includePrerequisites: false);

            // We don't check Depth Drops as a prereq for our exercise if that is a Basketball exercise and not a Soccer exercise.
            // But we do want to check exercises that our a part of the normal strength training  (non-SportsFocus) regimen.
            checkPrerequisitesFromQuery = Filters.FilterSportsFocus(checkPrerequisitesFromQuery, SportsOptions.SportsFocus, includeNone: true);
            checkPrerequisitesFromQuery = Filters.FilterExerciseType(checkPrerequisitesFromQuery, ExerciseTypeOptions.PrerequisiteExerciseType);
            
            // Make sure we have a user before we query for prerequisites.
            var checkPrerequisitesFrom = await checkPrerequisitesFromQuery
                .Select(a => new PrerequisitesQueryResults()
                {
                    UserExerciseProgression = a.UserExercise.Progression,
                    UserExerciseLastSeen = a.UserExercise.LastSeen,
                    UserExerciseVariationLastSeen = a.UserExerciseVariation.LastSeen,
                    ExerciseProficiency = a.Exercise.Proficiency,
                    ExerciseId = a.Exercise.Id,
                    ExerciseVariationProgression = a.ExerciseVariation.Progression
                }).ToListAsync();

            // Grab a list of non-filtered variations for all the exercises we grabbed.
            var eligibleExerciseIds = queryResults.Select(qr => qr.Exercise.Id).ToList();
            var allExercisesVariations = await CreateExerciseVariationsQuery(includeIntensities: false, includeInstructions: false, includePrerequisites: false)
                // We only need exercise variations for the exercises in our query result set.
                .Where(ev => eligibleExerciseIds.Contains(ev.Exercise.Id))
                .ToListAsync();

            foreach (var queryResult in queryResults)
            {
                if (queryResult.UserExercise == null || queryResult.UserExerciseVariation == null || queryResult.UserVariation == null)
                {
                    throw new NullReferenceException("User* values must be set before querying");
                }

                // Grab variations that are in the user's progression range. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsIgnored = allExercisesVariations
                    .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => ev.UserVariation.Ignore);

                // Grab variations that the user owns the necessary equipment for. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsMissingEquipment = allExercisesVariations
                    .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                    .Where(ev => ev.IsMinProgressionInRange && ev.IsMaxProgressionInRange)
                    .All(ev => !ev.UserOwnsEquipment);

                queryResult.EasierVariation = Tuple.Create(
                    allExercisesVariations
                        .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderByDescending(ev => ev.ExerciseVariation.Progression.Max)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.UserVariation?.Ignore == true : ev.UserVariation?.Ignore == false)
                        .FirstOrDefault(ev => ev.ExerciseVariation.Id != queryResult.ExerciseVariation.Id
                            && (
                                // Current progression is in range, choose the previous progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.ExerciseVariation.Progression.Max != null && ev.ExerciseVariation.Progression.Max <= queryResult.UserExercise.Progression)
                                // Current progression is out of range, choose the previous progression by looking at current exercise's min progression
                                || (!queryResult.IsMinProgressionInRange && ev.ExerciseVariation.Progression.Max != null && ev.ExerciseVariation.Progression.Max <= queryResult.ExerciseVariation.Progression.Min)
                            ))?
                        .Variation, 
                    !queryResult.IsMinProgressionInRange 
                        ? (queryResult.AllCurrentVariationsIgnored 
                            ? "Ignored" 
                            : queryResult.AllCurrentVariationsMissingEquipment 
                                ? "Missing Equipment" 
                                : null) 
                        : null
                );

                queryResult.HarderVariation = Tuple.Create(
                    allExercisesVariations
                        .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderBy(ev => ev.ExerciseVariation.Progression.Min)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.UserVariation?.Ignore == true : ev.UserVariation?.Ignore == false)
                        .FirstOrDefault(ev => ev.ExerciseVariation.Id != queryResult.ExerciseVariation.Id
                            && (
                                // Current progression is in range, choose the next progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.ExerciseVariation.Progression.Min != null && ev.ExerciseVariation.Progression.Min > queryResult.UserExercise.Progression)
                                // Current progression is out of range, choose the next progression by looking at current exercise's min progression
                                || (!queryResult.IsMaxProgressionInRange && ev.ExerciseVariation.Progression.Min != null && ev.ExerciseVariation.Progression.Min > queryResult.ExerciseVariation.Progression.Max)
                            ))?
                        .Variation, 
                    !queryResult.IsMaxProgressionInRange
                        ? (queryResult.AllCurrentVariationsIgnored
                            ? "Ignored"
                            : queryResult.AllCurrentVariationsMissingEquipment
                                ? "Missing Equipment"
                                : null)
                        : null
                );

                // The next variation in the exercise track based on variation progression levels
                queryResult.NextProgression = allExercisesVariations
                    // Stop at the lower bounds of variations    
                    .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.UserVariation?.Ignore == false)
                        .Select(ev => ev.ExerciseVariation.Progression.Min)
                    // Stop at the upper bounds of variations
                    .Union(allExercisesVariations
                        .Where(ev => ev.Exercise.Id == queryResult.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.UserVariation?.Ignore == false)
                        .Select(ev => ev.ExerciseVariation.Progression.Max)
                    )
                    .Where(mp => mp.HasValue && mp > queryResult.UserExercise.Progression)
                    .OrderBy(mp => mp - queryResult.UserExercise.Progression)
                    .FirstOrDefault();

                // Require the prerequisites show first
                var queryResultPrerequisiteExerciseIds = queryResult.Exercise.Prerequisites.Select(p => p.PrerequisiteExerciseId).ToList();
                if (!checkPrerequisitesFrom
                    // The prerequisite is in the list of filtered exercises, so that we don't see a rehab exercise as a prerequisite when strength training.
                    .Where(prereq => queryResultPrerequisiteExerciseIds.Contains(prereq.ExerciseId))
                    // The prerequisite falls in the range of the exercise's proficiency level
                    .Where(prereq => prereq.ExerciseProficiency >= prereq.ExerciseVariationProgression.MinOrDefault)
                    .Where(prereq => prereq.ExerciseProficiency < prereq.ExerciseVariationProgression.MaxOrDefault)
                    // User is at or past the required proficiency level.
                    .All(prereq => prereq.UserExerciseProgression >= prereq.ExerciseProficiency
                        // The prerequisite exercise has been seen in the past.
                        // We don't want to show Handstand Pushups before the user has seen Pushups.
                        && prereq.UserExerciseLastSeen > DateOnly.MinValue
                        // All of the prerequisite's proficiency variations have been seen in the past.
                        // We don't want to show Handstand Pushups before the user has seen Full Pushups.
                        && prereq.UserExerciseVariationLastSeen > DateOnly.MinValue)
                    ) 
                {
                    continue;
                }

                filteredResults.Add(queryResult);
            }

            if (!SelectionOptions.IgnoreProgressions)
            {
                // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
                filteredResults = filteredResults.GroupBy(i => i, new ExerciseComparer())
                                    .SelectMany(g => // LINQ is not the way to go about this...
                                                     // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                        g.Where(a => a.IsMinProgressionInRange && a.IsMaxProgressionInRange).NullIfEmpty()
                                            ?? g.Where(a => !a.IsMaxProgressionInRange /*&& Proficiency.AllowLesserProgressions*/)
                                                // Only grab lower progressions when all of the current variations are ignored.
                                                // It's possible a lack of equipment causes the current variation to not show.
                                                .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
                                                // FIXED: If two variations have the same max proficiency, should we select both? Yes
                                                .GroupBy(e => e.ExerciseVariation.Progression.MaxOrDefault).OrderByDescending(k => k.Key).Take(1).SelectMany(k => k).NullIfEmpty()
                                            // If there is no lesser progression, select the next higher variation.
                                            // We do this so the user doesn't get stuck at the beginning of an exercise track if they ignore the first variation instead of progressing.
                                            ?? g.Where(a => !a.IsMinProgressionInRange /*&& Proficiency.AllowGreaterProgressions*/)
                                                // Only grab higher progressions when all of the current variations are ignored.
                                                // It's possible a lack of equipment causes the current variation to not show.
                                                .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
                                                // FIXED: When filtering down to something like MovementPatterns,
                                                // ...if the next highest variation that passes the MovementPattern filter is higher than the next highest variation that doesn't,
                                                // ...then we will get a twice-as-difficult next variation.
                                                .Where(a => a.ExerciseVariation.Progression.MinOrDefault <= (g.Key.NextProgression ?? UserExercise.MaxUserProgression))
                                                // FIXED: If two variations have the same min proficiency, should we select both? Yes
                                                .GroupBy(e => e.ExerciseVariation.Progression.MinOrDefault).OrderBy(k => k.Key).Take(1).SelectMany(k => k)
                                    ).ToList();
            }
        }

        // OrderBy must come after query or you get duplicates.
        var orderedResults = filteredResults
            // Show exercises that the user has rarely seen
            .OrderBy(a => a.UserExercise == null ? DateOnly.MinValue : a.UserExercise.LastSeen)
            // Show variations that the user has rarely seen
            .ThenBy(a => a.UserExerciseVariation == null ? DateOnly.MinValue : a.UserExerciseVariation.LastSeen)
            // Mostly for the demo, show mostly random exercises
            .ThenBy(a => Guid.NewGuid())
            // Don't re-order the list on each read
            .ToList();

        var muscleTarget = MuscleGroup.MuscleTarget.Compile();
        var secondaryMuscleTarget = MuscleGroup.SecondaryMuscleTarget?.Compile();
        var finalResults = new List<QueryResults>();

        do
        {
            foreach (var exercise in orderedResults)
            {
                // Don't choose two variations of the same exercise
                if (SelectionOptions.UniqueExercises
                    && finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                {
                    continue;
                }

                // Don't choose exercises under our desired number of worked muscles
                if (MuscleGroup.AtLeastXMusclesPerExercise != null
                    && BitOperations.PopCount((ulong)muscleTarget(exercise)) < MuscleGroup.AtLeastXMusclesPerExercise)
                {
                    continue;
                }

                // Choose exercises that cover at least X muscles in the targeted muscles set
                if (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null)
                {
                    MuscleGroups unworkedMuscleGroups;
                    if (MuscleGroup.MuscleGroups != MuscleGroups.None)
                    {
                        unworkedMuscleGroups = EnumExtensions.GetSingleValues32<MuscleGroups>().Where(mg =>
                            // We are targeting this muscle group.    
                            MuscleGroup.MuscleGroups.HasFlag(mg)
                            // We have not already worked this muscle group once as our primary muscle target.
                            && !finalResults.WorkedMuscles(muscleTarget: muscleTarget).HasFlag(mg)
                            // We have not already worked this muscle group twice or more as our secondary muscle target.
                            && (secondaryMuscleTarget == null
                                || !finalResults.WorkedMusclesDict(muscleTarget: secondaryMuscleTarget)
                                    .Where(md => md.Value >= 2)
                                    .Any(kv => kv.Key == mg)
                                )
                        ).Aggregate(MuscleGroups.None, (curr, n) => curr | n);
                    }
                    else
                    {
                        unworkedMuscleGroups = EnumExtensions.GetSingleValues32<MuscleGroups>().Where(mg =>
                        {
                            // We are targeting this muscle group.    
                            var targeting = MuscleGroup.MuscleTargets.TryGetValue(mg, out int target) && target >= 0;
                            var primaryMusclesWorkedDict = finalResults.WorkedMusclesDict(muscleTarget: muscleTarget);
                            var alreadyWorkedPrimary = primaryMusclesWorkedDict[mg] >= target;
                            bool alreadyWorkedSecondary = false;
                            if (secondaryMuscleTarget != null)
                            {
                                var secondaryMusclesWorkedDict = finalResults.WorkedMusclesDict(muscleTarget: secondaryMuscleTarget);
                                // Secondary muscle groups are weighted less
                                alreadyWorkedSecondary = secondaryMusclesWorkedDict[mg] >= target * 2;
                            }

                            return targeting && !alreadyWorkedPrimary && !alreadyWorkedSecondary;
                        }).Aggregate(MuscleGroups.None, (curr, n) => curr | n);
                    }

                    // We've already worked all unique muscles
                    if (unworkedMuscleGroups == MuscleGroups.None)
                    {
                        break;
                    }

                    // The exercise does not work enough unique muscles that we are trying to target.
                    if (BitOperations.PopCount((ulong)(muscleTarget(exercise) & unworkedMuscleGroups)) < Math.Max(1, MuscleGroup.AtLeastXUniqueMusclesPerExercise.Value))
                    {
                        continue;
                    }

                    // Don't choose any exercise that works one of the muscles groups we've worked too much.
                    var workedTooMuchMuscles = MuscleGroup.MuscleTargets.Where(mg => mg.Value < 0).Aggregate(MuscleGroups.None, (curr, n) => curr | n.Key);
                    if (BitOperations.PopCount((ulong)(muscleTarget(exercise) & workedTooMuchMuscles)) > 0)
                    {
                        continue;
                    }
                }

                // Choose exercises that cover a unique movement pattern
                if (MovementPattern.MovementPatterns.HasValue && MovementPattern.IsUnique)
                {
                    var unworkedMovementPatterns = EnumExtensions.GetValuesExcluding32(Models.Exercise.MovementPattern.None, Models.Exercise.MovementPattern.All)
                        // The movement pattern is in our list of movement patterns to work
                        .Where(v => MovementPattern.MovementPatterns.Value.HasFlag(v))
                        // The movement pattern has not yet been worked
                        .Where(mp => !finalResults.Any(r => mp.HasAnyFlag32(r.Variation.MovementPattern)));

                    // We've already worked all unique movement patterns
                    if (!unworkedMovementPatterns.Any())
                    {
                        break;
                    }

                    // If none of the unworked movement patterns match up with the variation's movement patterns
                    if (!unworkedMovementPatterns.Any(mp => mp.HasAnyFlag32(exercise.Variation.MovementPattern)))
                    {
                        continue;
                    }
                }

                finalResults.Add(new QueryResults(User, exercise.Exercise, exercise.Variation, exercise.ExerciseVariation, exercise.UserExercise, exercise.UserExerciseVariation, exercise.UserVariation, exercise.EasierVariation, exercise.HarderVariation));
            }
        }
        while (
            // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
            // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
            (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null && --MuscleGroup.AtLeastXUniqueMusclesPerExercise >= 1)
        // Reverse
        //|| (MuscleGroup.AtMostXUniqueMusclesPerExercise != null && ++MuscleGroup.AtMostXUniqueMusclesPerExercise <= 9) // FIXME 9
        );

        return OrderByOptions.OrderBy switch
        {
            OrderBy.None => finalResults,
            OrderBy.Name => finalResults.OrderBy(vm => vm.Variation.Name).ToList(),
            OrderBy.Progression => finalResults.Take(OrderByOptions.SkipCount).Concat(finalResults.Skip(OrderByOptions.SkipCount)
                                                   .OrderBy(vm => vm.ExerciseVariation.Progression.Min)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max == null)
                                                   .ThenBy(vm => vm.ExerciseVariation.Progression.Max)
                                                   .ThenBy(vm => vm.Variation.Name))
                                                   .ToList(),
            OrderBy.CoreLast => finalResults.Take(OrderByOptions.SkipCount).Concat(finalResults.Skip(OrderByOptions.SkipCount)
                                                    .OrderBy(vm => BitOperations.PopCount((ulong)(muscleTarget(vm) & MuscleGroups.Core)))
                                                    .ThenByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)) - BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups)))
                                                    .ThenBy(vm => BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups))))
                                                    .ToList(),
            OrderBy.MuscleTarget => finalResults.Take(OrderByOptions.SkipCount).Concat(finalResults.Skip(OrderByOptions.SkipCount)
                                                    .OrderByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)) - BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups)))
                                                    .ThenBy(vm => BitOperations.PopCount((ulong)muscleTarget(vm).UnsetFlag32(MuscleGroup.MuscleGroups))))
                                                    .ToList(),
            _ => finalResults,
        };
    }
}
