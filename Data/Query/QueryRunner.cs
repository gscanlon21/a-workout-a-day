using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Code.Extensions;
using Data.Dtos.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Models;
using Data.Query.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Numerics;
using System.Security.Cryptography;

namespace Data.Query;

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
        public UserExercise UserExercise { get; init; } = null!;
        public UserVariation UserVariation { get; init; } = null!;
        public bool UserOwnsEquipment { get; init; }
        public bool IsMinProgressionInRange { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    private class InProgressQueryResults :
        IExerciseVariationCombo
    {
        public InProgressQueryResults(ExerciseVariationsQueryResults queryResult)
        {
            Exercise = queryResult.Exercise;
            Variation = queryResult.Variation;
            UserExercise = queryResult.UserExercise;
            UserVariation = queryResult.UserVariation;
            UserOwnsEquipment = queryResult.UserOwnsEquipment;
            IsMinProgressionInRange = queryResult.IsMinProgressionInRange;
            IsMaxProgressionInRange = queryResult.IsMaxProgressionInRange;
            ExercisePrerequisites = queryResult.Exercise.Prerequisites.Select(p => new ExercisePrerequisiteDto(p)).ToList();
        }

        public Exercise Exercise { get; }
        public Variation Variation { get; }
        public UserExercise? UserExercise { get; set; }
        public UserVariation? UserVariation { get; set; }
        public IList<ExercisePrerequisiteDto> ExercisePrerequisites { get; init; } = null!;
        public bool UserOwnsEquipment { get; }
        public bool IsMinProgressionInRange { get; }
        public bool IsMaxProgressionInRange { get; }
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;

        public bool AllCurrentVariationsIgnored { get; set; }
        public bool AllCurrentVariationsMissingEquipment { get; set; }
        public (string? name, string? reason) EasierVariation { get; set; }
        public (string? name, string? reason) HarderVariation { get; set; }
        public int? NextProgression { get; set; }

        public override int GetHashCode() => HashCode.Combine(Exercise.Id);

        public override bool Equals(object? obj) => obj is InProgressQueryResults other
            && other.Exercise.Id == Exercise.Id;
    }

    [DebuggerDisplay("{ExerciseId}: {VariationName}")]
    private class AllVariationsQueryResults
    {
        public AllVariationsQueryResults(ExerciseVariationsQueryResults queryResult)
        {
            ExerciseId = queryResult.Exercise.Id;
            VariationId = queryResult.Variation.Id;
            VariationName = queryResult.Variation.Name;
            VariationProgression = queryResult.Variation.Progression;
            UserOwnsEquipment = queryResult.UserOwnsEquipment;
            IsMinProgressionInRange = queryResult.IsMinProgressionInRange;
            IsMaxProgressionInRange = queryResult.IsMaxProgressionInRange;
            IsIgnored = queryResult.UserVariation?.Ignore ?? false;
        }

        public int ExerciseId { get; }
        public int VariationId { get; }
        public string VariationName { get; }
        public Progression VariationProgression { get; }
        public bool IsIgnored { get; }
        public bool UserOwnsEquipment { get; }
        public bool IsMinProgressionInRange { get; }
        public bool IsMaxProgressionInRange { get; }
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;
    }

    [DebuggerDisplay("{ExerciseId}")]
    private class PrerequisitesQueryResults
    {
        public PrerequisitesQueryResults(ExerciseVariationsQueryResults queryResult)
        {
            ExerciseId = queryResult.Exercise.Id;
            VariationProgression = queryResult.Variation.Progression;
            UserOwnsEquipment = queryResult.UserOwnsEquipment;
            IsMinProgressionInRange = queryResult.IsMinProgressionInRange;
            IsMaxProgressionInRange = queryResult.IsMaxProgressionInRange;
            UserVariationLastSeen = queryResult.UserVariation?.LastSeen ?? DateOnly.MinValue;
            UserExerciseLastSeen = queryResult.UserExercise?.LastSeen ?? DateOnly.MinValue;
            UserExerciseProgression = queryResult.UserExercise?.Progression ?? UserConsts.MinUserProgression;
        }

        public int ExerciseId { get; }
        public int UserExerciseProgression { get; }
        public DateOnly UserExerciseLastSeen { get; }
        public DateOnly UserVariationLastSeen { get; }
        public Progression VariationProgression { get; }
        public bool UserOwnsEquipment { get; }
        public bool IsMinProgressionInRange { get; }
        public bool IsMaxProgressionInRange { get; }
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;
    }

    private Section Section { get; }
    public required UserOptions UserOptions { get; init; }
    public required SelectionOptions SelectionOptions { get; init; }
    public required ExclusionOptions ExclusionOptions { get; init; }
    public required ExerciseOptions ExerciseOptions { get; init; }
    public required MovementPatternOptions MovementPattern { get; init; }
    public required MuscleGroupOptions MuscleGroup { get; init; }
    public required ExerciseTypeOptions ExerciseTypeOptions { get; init; }
    public required JointsOptions JointsOptions { get; init; }
    public required SportsOptions SportsOptions { get; init; }
    public required EquipmentOptions EquipmentOptions { get; init; }
    public required ExerciseFocusOptions ExerciseFocusOptions { get; init; }
    public required MuscleContractionsOptions MuscleContractionsOptions { get; init; }
    public required MuscleMovementOptions MuscleMovementOptions { get; init; }

    public QueryRunner(Section section)
    {
        Section = section;
    }

    private IQueryable<ExercisesQueryResults> CreateExercisesQuery(CoreContext context, bool includePrerequisites)
    {
        var query = context.Exercises.IgnoreQueryFilters().TagWith(nameof(CreateExercisesQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includePrerequisites)
        {
            query = query.Include(e => e.Prerequisites).ThenInclude(p => p.PrerequisiteExercise);
        }

        return query.Select(i => new ExercisesQueryResults()
        {
            Exercise = i,
            UserExercise = i.UserExercises.First(ue => ue.UserId == UserOptions.Id)
        });
    }

    private IQueryable<VariationsQueryResults> CreateVariationsQuery(CoreContext context, bool includeInstructions)
    {
        var query = context.Variations.IgnoreQueryFilters().TagWith(nameof(CreateVariationsQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includeInstructions)
        {
            query = query
                // Instruction equipment is auto included. Instruction location is auto included.
                .Include(i => i.Instructions.Where(d => d.DisabledReason == null).Where(eg => eg.Parent == null))
                    .ThenInclude(eg => eg.Children.Where(d => d.DisabledReason == null));
        }

        return query.Select(v => new VariationsQueryResults()
        {
            Variation = v,
            UserVariation = v.UserVariations.First(uv => uv.UserId == UserOptions.Id && uv.Section.HasFlag(Section) && Section != Section.None)
        });
    }

    private IQueryable<ExerciseVariationsQueryResults> CreateExerciseVariationsQuery(CoreContext context, bool includeInstructions, bool includePrerequisites)
    {
        return CreateExercisesQuery(context, includePrerequisites: includePrerequisites)
            .Join(CreateVariationsQuery(context, includeInstructions: includeInstructions),
                o => o.Exercise.Id, i => i.Variation.ExerciseId, (o, i) => new
                {
                    o.Exercise,
                    o.UserExercise,
                    i.Variation,
                    i.UserVariation,
                })
            .Select(a => new ExerciseVariationsQueryResults()
            {
                Exercise = a.Exercise,
                Variation = a.Variation,
                UserExercise = a.UserExercise,
                UserVariation = a.UserVariation,
                // Out of range when the exercise is too difficult for the user
                IsMinProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no minimum 
                    || a.Variation.Progression.Min == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise == null ? (UserOptions.IsNewToFitness ? UserConsts.MinUserProgression : UserConsts.MidUserProgression) : a.UserExercise.Progression) >= a.Variation.Progression.Min,
                // Out of range when the exercise is too easy for the user
                IsMaxProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no maximum
                    || a.Variation.Progression.Max == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise == null ? (UserOptions.IsNewToFitness ? UserConsts.MinUserProgression : UserConsts.MidUserProgression) : a.UserExercise.Progression) < a.Variation.Progression.Max,
                // User owns at least one equipment in at least one of the optional equipment groups
                UserOwnsEquipment = UserOptions.NoUser
                    // There is an instruction that does not require any equipment
                    || a.Variation.DefaultInstruction != null
                    // Out of the instructions that require equipment, the user owns the equipment for the root instruction and the root instruction can be done on its own, or the user own the equipment of the child instructions. 
                    || a.Variation.Instructions.Where(i => i.Parent == null).Any(peg =>
                        // User owns equipment for the root instruction 
                        (peg.Equipment & UserOptions.Equipment) != 0
                        && (
                            // Root instruction can be done on its own
                            peg.Link != null
                            // Or the user owns the equipment for the child instructions. HasAnyFlag
                            || peg.Children.Any(ceg => (ceg.Equipment & UserOptions.Equipment) != 0)
                        )
                    )
            });
    }

    private IQueryable<ExerciseVariationsQueryResults> CreateFilteredExerciseVariationsQuery(CoreContext context, bool includeIntensities, bool includeInstructions, bool includePrerequisites, bool ignoreExclusions = false)
    {
        var filteredQuery = CreateExerciseVariationsQuery(context,
                includeInstructions: includeInstructions,
                includePrerequisites: includePrerequisites)
            .TagWith(nameof(CreateFilteredExerciseVariationsQuery))
            // Filter down to variations the user owns equipment for
            .Where(vm => vm.UserOwnsEquipment)
            // Don't grab exercises that the user wants to ignore
            .Where(vm => vm.UserExercise.Ignore != true)
            // Don't grab variations that the user wants to ignore
            .Where(vm => vm.UserVariation.Ignore != true);

        if (!ignoreExclusions)
        {
            filteredQuery = filteredQuery
                // Don't grab groups that we want to ignore
                .Where(vm => (ExclusionOptions.ExerciseGroups & vm.Exercise.Groups) == 0)
                // Don't grab exercises that we want to ignore
                .Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id))
                // Don't grab variations that we want to ignore.
                .Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));
        }

        if (!UserOptions.NoUser)
        {
            // Don't show dangerous exercises when the user is new to fitness.
            filteredQuery = filteredQuery.Where(vm => !UserOptions.IsNewToFitness || !vm.Variation.UseCaution);
        }

        return filteredQuery;
    }

    /// <summary>
    /// Queries the db for the data
    /// </summary>
    public async Task<IList<QueryResults>> Query(IServiceScopeFactory factory, int take = int.MaxValue)
    {
        using var scope = factory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var filteredQuery = CreateFilteredExerciseVariationsQuery(context, includeIntensities: true, includeInstructions: true, includePrerequisites: true);

        filteredQuery = Filters.FilterExerciseType(filteredQuery, ExerciseTypeOptions.ExerciseType);
        filteredQuery = Filters.FilterJoints(filteredQuery, JointsOptions.Joints, include: true);
        filteredQuery = Filters.FilterJoints(filteredQuery, JointsOptions.ExcludeJoints, include: false);
        filteredQuery = Filters.FilterExercises(filteredQuery, ExerciseOptions.ExerciseIds);
        filteredQuery = Filters.FilterVariations(filteredQuery, ExerciseOptions.VariationIds);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExerciseFocus);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExcludeExerciseFocus, exclude: true);
        filteredQuery = Filters.FilterSportsFocus(filteredQuery, SportsOptions.SportsFocus);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPattern.MovementPatterns);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.MuscleGroups.Aggregate(MuscleGroups.None, (curr2, n2) => curr2 | n2), include: true, MuscleGroup.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, UserOptions.ExcludeRecoveryMuscle, include: false, UserOptions.ExcludeRecoveryMuscleTarget);
        filteredQuery = Filters.FilterEquipmentIds(filteredQuery, EquipmentOptions.Equipment);
        filteredQuery = Filters.FilterMuscleContractions(filteredQuery, MuscleContractionsOptions.MuscleContractions);
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);

        var queryResults = await filteredQuery.Select(a => new InProgressQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();

        var filteredResults = new List<InProgressQueryResults>();
        if (UserOptions.NoUser)
        {
            filteredResults = queryResults;
        }
        else
        {
            // Grab a list of non-filtered variations for all the exercises we grabbed.
            // We only need exercise variations for the exercises in our query result set.
            var allExercisesVariations = await Filters.FilterExercises(CreateExerciseVariationsQuery(context, includeInstructions: false, includePrerequisites: false), queryResults.Select(qr => qr.Exercise.Id).ToList())
                .Select(a => new AllVariationsQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();

            var checkPrerequisitesFrom = new List<PrerequisitesQueryResults>();
            if (!UserOptions.IgnorePrerequisites)
            {
                // Grab a half-filtered list of exercises to check the prerequisites from.
                // We don't want to see a rehab exercise as a prerequisite when strength training.
                // We do want to see Planks (isometric) and Dynamic Planks (isotonic) as a prereq for Mountain Climbers (plyo).
                var checkPrerequisitesFromQuery = CreateFilteredExerciseVariationsQuery(context, includeIntensities: false, includeInstructions: false, includePrerequisites: false, ignoreExclusions: true);

                // We don't check Depth Drops as a prereq for our exercise if that is a Basketball exercise and not a Soccer exercise.
                // But we do want to check exercises that our a part of the normal strength training  (non-SportsFocus) regimen.
                checkPrerequisitesFromQuery = Filters.FilterSportsFocus(checkPrerequisitesFromQuery, SportsOptions.SportsFocus, includeNone: true);
                checkPrerequisitesFromQuery = Filters.FilterExerciseType(checkPrerequisitesFromQuery, ExerciseTypeOptions.PrerequisiteExerciseType);

                // Further filter down the exercises to those that match our query results.
                checkPrerequisitesFromQuery = Filters.FilterExercises(checkPrerequisitesFromQuery, queryResults.SelectMany(qr => qr.ExercisePrerequisites.Select(p => p.Id)).ToList());

                // Make sure we have a user before we query for prerequisites.
                checkPrerequisitesFrom = await checkPrerequisitesFromQuery.Select(a => new PrerequisitesQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
            }

            await AddMissingUserRecords(context, queryResults);
            foreach (var queryResult in queryResults)
            {
                // Grab variations that are in the user's progression range. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsIgnored = allExercisesVariations
                    .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                    .Where(ev => ev.IsProgressionInRange)
                    .All(ev => ev.IsIgnored) && allExercisesVariations.Any();

                // Grab variations that the user owns the necessary equipment for. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsMissingEquipment = allExercisesVariations
                    .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                    .Where(ev => ev.IsProgressionInRange)
                    .All(ev => !ev.UserOwnsEquipment) && allExercisesVariations.Any();

                queryResult.EasierVariation = (
                    allExercisesVariations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderByDescending(ev => ev.VariationProgression.Max)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.IsIgnored == true : ev.IsIgnored == false)
                        .FirstOrDefault(ev => ev.VariationId != queryResult.Variation.Id
                            && (
                                // Current progression is in range, choose the previous progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.VariationProgression.Max != null && ev.VariationProgression.Max <= queryResult.UserExercise!.Progression)
                                // Current progression is out of range, choose the previous progression by looking at current exercise's min progression
                                || (!queryResult.IsMinProgressionInRange && ev.VariationProgression.Max != null && ev.VariationProgression.Max <= queryResult.Variation.Progression.Min)
                            ))?
                        .VariationName,
                    !queryResult.IsMinProgressionInRange
                        ? (queryResult.AllCurrentVariationsIgnored
                            ? "Ignored"
                            : queryResult.AllCurrentVariationsMissingEquipment
                                ? "Missing Equipment"
                                : null)
                        : null
                );

                queryResult.HarderVariation = (
                    allExercisesVariations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderBy(ev => ev.VariationProgression.Min)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.IsIgnored == true : ev.IsIgnored == false)
                        .FirstOrDefault(ev => ev.VariationId != queryResult.Variation.Id
                            && (
                                // Current progression is in range, choose the next progression by looking at the user's current progression level
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.VariationProgression.Min != null && ev.VariationProgression.Min > queryResult.UserExercise!.Progression)
                                // Current progression is out of range, choose the next progression by looking at current exercise's min progression
                                || (!queryResult.IsMaxProgressionInRange && ev.VariationProgression.Min != null && ev.VariationProgression.Min >= queryResult.Variation.Progression.Max)
                            ))?
                        .VariationName,
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
                    .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.IsIgnored == false)
                        .Select(ev => ev.VariationProgression.Min)
                    // Stop at the upper bounds of variations
                    .Union(allExercisesVariations
                        .Where(ev => ev.ExerciseId == queryResult.Exercise.Id)
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.IsIgnored == false)
                        .Select(ev => ev.VariationProgression.Max)
                    )
                    .Where(mp => mp.HasValue && mp > queryResult.UserExercise!.Progression)
                    .OrderBy(mp => mp - queryResult.UserExercise!.Progression)
                    .FirstOrDefault();

                if (!PrerequisitesPass(queryResult, checkPrerequisitesFrom))
                {
                    continue;
                }

                filteredResults.Add(queryResult);
            }

            if (!UserOptions.IgnoreProgressions)
            {
                // Try choosing variations that have a max progression above the user's progression. Fallback to an easier variation if one does not exist.
                filteredResults = filteredResults.GroupBy(i => i)
                                    // LINQ is not the way to go about this...
                                    .SelectMany(g =>
                                        // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation
                                        g.Where(a => a.IsMinProgressionInRange && a.IsMaxProgressionInRange).NullIfEmpty()
                                            ?? g.Where(a => !a.IsMaxProgressionInRange /*&& Proficiency.AllowLesserProgressions*/)
                                                // Only grab lower progressions when all of the current variations are ignored.
                                                // It's possible a lack of equipment causes the current variation to not show.
                                                .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
                                                // FIXED: If two variations have the same max proficiency, should we select both? Yes
                                                .GroupBy(e => e.Variation.Progression.MaxOrDefault).OrderByDescending(k => k.Key).Take(1).SelectMany(k => k).NullIfEmpty()
                                            // If there is no lesser progression, select the next higher variation.
                                            // We do this so the user doesn't get stuck at the beginning of an exercise track if they ignore the first variation instead of progressing.
                                            ?? g.Where(a => !a.IsMinProgressionInRange /*&& Proficiency.AllowGreaterProgressions*/)
                                                // Only grab higher progressions when all of the current variations are ignored.
                                                // It's possible a lack of equipment causes the current variation to not show.
                                                .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment)
                                                // FIXED: When filtering down to something like MovementPatterns,
                                                // ...if the next highest variation that passes the MovementPattern filter is higher than the next highest variation that doesn't,
                                                // ...then we will get a twice-as-difficult next variation.
                                                .Where(a => a.Variation.Progression.MinOrDefault <= (g.Key.NextProgression ?? UserConsts.MaxUserProgression))
                                                // FIXED: If two variations have the same min proficiency, should we select both? Yes
                                                .GroupBy(e => e.Variation.Progression.MinOrDefault).OrderBy(k => k.Key).Take(1).SelectMany(k => k)
                                    ).ToList();
            }
        }

        // OrderBy must come after the query or you get cartesian explosion.
        var orderedResults = filteredResults
            // Show exercise variations that the user has rarely seen.
            // Adding the two in case there is a warmup and main variation in the same exercise.
            // ... Otherwise, since the warmup section is always choosen first, the last seen date is always updated and the main variation is rarely choosen.
            .OrderBy(a => a.UserExercise?.LastSeen.DayNumber + a.UserVariation?.LastSeen.DayNumber)
            // Mostly for the demo, show mostly random exercises.
            // NOTE: When the two variation's LastSeen dates are the same:
            // ... The RefreshAfterXWeeks will prevent the LastSeen date from updating
            // ... and we may see two randomly alternating exercises for the RefreshAfterXWeeks duration.
            .ThenBy(_ => RandomNumberGenerator.GetInt32(Int32.MaxValue))
            // Don't re-order the list on each read
            .ToList();

        var muscleTarget = MuscleGroup.MuscleTarget.Compile();
        var secondaryMuscleTarget = MuscleGroup.SecondaryMuscleTarget?.Compile();
        var finalResults = new List<QueryResults>();

        // This is a hack to make sure the user does not get stuck from seeing certain exercises if, for example, a prerequisite only works one muscle group and that muscle groups is worked otherwise by compound exercises.
        var leastSeenExercise = orderedResults.FirstOrDefault();
        if (!UserOptions.NoUser
            && UserOptions.CreatedDate < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1)
            && MuscleGroup.AtLeastXUniqueMusclesPerExercise != null
            && leastSeenExercise?.UserVariation != null
            && leastSeenExercise.UserVariation.LastSeen < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserOptions.RefreshExercisesAfterXWeeks + 1)))
        {
            var overworkedMuscleGroups = GetOverworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);
            if (!overworkedMuscleGroups.Any(mg => muscleTarget(leastSeenExercise).HasAnyFlag32(mg)) && MuscleGroup.MuscleGroups.Any(mg => muscleTarget(leastSeenExercise).HasAnyFlag32(mg)))
            {
                finalResults.Add(new QueryResults(Section, leastSeenExercise.Exercise, leastSeenExercise.Variation, leastSeenExercise.UserExercise, leastSeenExercise.UserVariation, leastSeenExercise.ExercisePrerequisites, leastSeenExercise.EasierVariation, leastSeenExercise.HarderVariation));
            }
        }

        do
        {
            foreach (var exercise in orderedResults)
            {
                // Don't choose two variations of the same exercise.
                if (SelectionOptions.UniqueExercises
                    && finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                {
                    continue;
                }

                // Don't choose two variations of the same group.
                if (SelectionOptions.UniqueExercises
                    && (finalResults.Aggregate(ExerciseGroup.None, (curr, n) => curr | n.Exercise.Groups) & exercise.Exercise.Groups) != 0)
                {
                    continue;
                }

                // Don't choose exercises under our desired number of worked muscles.
                if (MuscleGroup.AtLeastXMusclesPerExercise != null
                    && BitOperations.PopCount((ulong)muscleTarget(exercise)) < MuscleGroup.AtLeastXMusclesPerExercise)
                {
                    continue;
                }

                // Don't overwork muscle groups.
                var overworkedMuscleGroups = GetOverworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);
                if (overworkedMuscleGroups.Any(mg => muscleTarget(exercise).HasAnyFlag32(mg)))
                {
                    continue;
                }

                // Choose exercises that cover at least X muscles in the targeted muscles set.
                if (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null)
                {
                    var unworkedMuscleGroups = GetUnworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);

                    // We've already worked all unique muscles
                    if (!unworkedMuscleGroups.Any())
                    {
                        break;
                    }

                    // The exercise does not work enough unique muscles that we are trying to target.
                    if (unworkedMuscleGroups.Count(mg => muscleTarget(exercise).HasAnyFlag32(mg)) < Math.Max(1, MuscleGroup.AtLeastXUniqueMusclesPerExercise.Value))
                    {
                        continue;
                    }
                }

                // Choose exercises that cover a unique movement pattern.
                if (MovementPattern.MovementPatterns.HasValue && MovementPattern.IsUnique)
                {
                    var unworkedMovementPatterns = EnumExtensions.GetValuesExcluding32(Core.Models.Exercise.MovementPattern.None, Core.Models.Exercise.MovementPattern.All)
                        // The movement pattern is in our list of movement patterns to work.
                        .Where(v => MovementPattern.MovementPatterns.Value.HasFlag(v))
                        // The movement pattern has not yet been worked.
                        .Where(mp => !finalResults.Any(r => mp.HasAnyFlag32(r.Variation.MovementPattern)));

                    // We've already worked all unique movement patterns.
                    if (!unworkedMovementPatterns.Any())
                    {
                        break;
                    }

                    // If none of the unworked movement patterns match up with the variation's movement patterns.
                    if (!unworkedMovementPatterns.Any(mp => mp.HasAnyFlag32(exercise.Variation.MovementPattern)))
                    {
                        continue;
                    }
                }

                finalResults.Add(new QueryResults(Section, exercise.Exercise, exercise.Variation, exercise.UserExercise, exercise.UserVariation, exercise.ExercisePrerequisites, exercise.EasierVariation, exercise.HarderVariation));
            }
        }
        // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
        // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
        while (MuscleGroup.AtLeastXUniqueMusclesPerExercise != null && --MuscleGroup.AtLeastXUniqueMusclesPerExercise >= 1);

        // REFACTORME
        return Section switch
        {
            Section.None => finalResults.Take(take)
                // Not in a workout context, order by progression levels.
                .OrderBy(vm => vm.Variation.Progression.Min)
                .ThenBy(vm => vm.Variation.Progression.Max == null)
                .ThenBy(vm => vm.Variation.Progression.Max)
                .ThenBy(vm => vm.Variation.Name)
                .ToList(),
            Section.Debug => finalResults
                // Not in a workout context, order by progression levels.
                .GroupBy(vm => vm.Exercise)
                .Take(take)
                .SelectMany(vm => vm)
                .OrderBy(vm => vm.Variation.Progression.Min)
                .ThenBy(vm => vm.Variation.Progression.Max == null)
                .ThenBy(vm => vm.Variation.Progression.Max)
                .ThenBy(vm => vm.Variation.Name)
                .ToList(),
            Section.WarmupRaise => finalResults.Take(take)
                // Order by least expected difficulty first.
                .OrderBy(vm => BitOperations.PopCount((ulong)muscleTarget(vm)))
                .ToList(),
            Section.Core => finalResults.Take(take)
                // Show exercises that work a muscle target we want more of first.
                .OrderByDescending(vm => BitOperations.PopCount((ulong)(muscleTarget(vm) & MuscleGroup.MuscleTargets.Where(mt => mt.Value > 1).Aggregate(MuscleGroups.None, (curr, n) => curr | n.Key))))
                // Then show exercise variations that the user has rarely seen.
                // Adding the two in case there is a warmup and main variation in the same exercise.
                // ... Otherwise, since the warmup section is always choosen first, the last seen date is always updated and the main variation is rarely choosen.
                .ThenBy(a => a.UserExercise?.LastSeen.DayNumber + a.UserVariation?.LastSeen.DayNumber)
                .ToList(),
            Section.Accessory => finalResults.Take(take)
                // Core exercises last.
                .OrderBy(vm => BitOperations.PopCount((ulong)(muscleTarget(vm) & MuscleGroups.Core)) >= 2)
                // Then by hardest expected difficulty.
                .ThenByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)))
                .ToList(),
            Section.Functional => finalResults.Take(take)
                // Plyometrics first.
                .OrderByDescending(vm => vm.Variation.MuscleMovement.HasFlag(MuscleMovement.Plyometric))
                // Core exercises last.
                .ThenBy(vm => BitOperations.PopCount((ulong)(muscleTarget(vm) & MuscleGroups.Core)) >= 2)
                // Then by hardest expected difficulty.
                .ThenByDescending(vm => BitOperations.PopCount((ulong)muscleTarget(vm)))
                .ToList(),
            _ => finalResults.Take(take).ToList() // We are in a workout context, keep the result order.
        };
    }

    private static bool PrerequisitesPass(InProgressQueryResults queryResult, IList<PrerequisitesQueryResults> checkPrerequisitesFrom)
    {
        foreach (var exercisePrerequisite in queryResult.ExercisePrerequisites)
        {
            // Require the prerequisites show first.
            if (!checkPrerequisitesFrom
                // The prerequisite is in the list of filtered exercises, so that we don't see a rehab exercise as a prerequisite when strength training.
                .Where(prereq => exercisePrerequisite.Id == prereq.ExerciseId)
                // The prerequisite falls in the range of the exercise's proficiency level.
                .Where(prereq => exercisePrerequisite.Proficiency >= prereq.VariationProgression.MinOrDefault)
                .Where(prereq => exercisePrerequisite.Proficiency < prereq.VariationProgression.MaxOrDefault)
                // User can do the prerequisite with their current equipment.
                .Where(prereq => prereq.UserOwnsEquipment)
                .All(prereq => (
                        // User is at the required proficiency level.
                        prereq.UserExerciseProgression == exercisePrerequisite.Proficiency
                        // The prerequisite exercise has been seen in the past.
                        // We don't want to show Handstand Pushups before the user has seen Pushups.
                        && prereq.UserExerciseLastSeen > DateOnly.MinValue
                        // All of the prerequisite's proficiency variations have been seen in the past.
                        // We don't want to show Handstand Pushups before the user has seen Full Pushups.
                        && prereq.UserVariationLastSeen > DateOnly.MinValue
                    ) || (
                        // User is past the required proficiency level.
                        prereq.UserExerciseProgression > exercisePrerequisite.Proficiency
                        // The prerequisite exercise has been seen in the past.
                        // We don't want to show Handstand Pushups before the user has seen Pushups.
                        && prereq.UserExerciseLastSeen > DateOnly.MinValue
                    )
                ))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Reference updates to QueryResult.UserExercise and QueryResult.UserVariation to set them to default and save to db if they are null.
    /// </summary>
    private async Task AddMissingUserRecords(CoreContext context, IList<InProgressQueryResults> queryResults)
    {
        // User is not viewing a newsletter, don't log.
        if (Section == Section.None)
        {
            return;
        }

        var exercisesUpdated = new HashSet<UserExercise>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserExercise == null))
        {
            queryResult.UserExercise = new UserExercise()
            {
                ExerciseId = queryResult.Exercise.Id,
                UserId = UserOptions.Id,
                Progression = UserOptions.IsNewToFitness ? UserConsts.MinUserProgression : UserConsts.MidUserProgression
            };

            if (exercisesUpdated.Add(queryResult.UserExercise))
            {
                context.UserExercises.Add(queryResult.UserExercise);
            }
        }

        var variationsUpdated = new HashSet<UserVariation>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserVariation == null))
        {
            queryResult.UserVariation = new UserVariation()
            {
                VariationId = queryResult.Variation.Id,
                UserId = UserOptions.Id,
                Section = Section
            };

            if (variationsUpdated.Add(queryResult.UserVariation))
            {
                context.UserVariations.Add(queryResult.UserVariation);
            }
        }

        await context.SaveChangesAsync();
    }

    private IList<MuscleGroups> GetUnworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, Func<IExerciseVariationCombo, MuscleGroups>? secondaryMuscleTarget = null)
    {
        // Not using MuscleGroups because MuscleTargets can contain unions 
        return MuscleGroup.MuscleTargets.Where(kv =>
        {
            // We are targeting this muscle group.
            var workedCount = finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            return workedCount < kv.Value && MuscleGroup.MuscleGroups.Any(mg => kv.Key.HasFlag(mg));
        }).Select(kv => kv.Key).ToList();
    }

    private IList<MuscleGroups> GetOverworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MuscleGroups> muscleTarget, Func<IExerciseVariationCombo, MuscleGroups>? secondaryMuscleTarget = null)
    {
        // Not using MuscleGroups because MuscleTargets can contain unions.
        return MuscleGroup.MuscleTargets.Where(kv =>
        {
            // We have not overworked this muscle group.
            var workedCount = finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            return workedCount > kv.Value;
        }).Select(kv => kv.Key).ToList();
    }
}
