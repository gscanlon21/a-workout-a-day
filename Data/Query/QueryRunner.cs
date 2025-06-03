using Core.Dtos.Exercise;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Code.Extensions;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Query.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Cryptography;

namespace Data.Query;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
public class QueryRunner(Section section)
{
    [DebuggerDisplay("{Exercise}")]
    private class ExercisesQueryResults
    {
        public required Exercise Exercise { get; init; }
        public required UserExercise UserExercise { get; init; }
        public IList<ExercisePrerequisiteDto> Prerequisites { get; init; } = [];
        public IList<ExercisePrerequisiteDto> Postrequisites { get; init; } = [];
    }

    [DebuggerDisplay("{Variation}")]
    private class VariationsQueryResults
    {
        public required Variation Variation { get; init; }
        public required UserVariation UserVariation { get; init; }
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    private class ExerciseVariationsQueryResults : IExerciseVariationCombo
    {
        public required Exercise Exercise { get; init; }
        public required Variation Variation { get; init; }
        public required UserExercise UserExercise { get; init; }
        public required UserVariation UserVariation { get; init; }
        public required IList<ExercisePrerequisiteDto> Prerequisites { get; init; }
        public required IList<ExercisePrerequisiteDto> Postrequisites { get; init; }
        public bool IsMinProgressionInRange { get; init; }
        public bool IsMaxProgressionInRange { get; init; }
        public bool UserOwnsEquipment { get; init; }
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    private class InProgressQueryResults(ExerciseVariationsQueryResults queryResult)
        : IExerciseVariationCombo
    {
        public Exercise Exercise { get; } = queryResult.Exercise;
        public Variation Variation { get; } = queryResult.Variation;
        public UserExercise? UserExercise { get; set; } = queryResult.UserExercise;
        public UserVariation? UserVariation { get; set; } = queryResult.UserVariation;

        public IList<ExercisePrerequisiteDto> Prerequisites { get; init; } = queryResult.Prerequisites;
        public IList<ExercisePrerequisiteDto> Postrequisites { get; init; } = queryResult.Postrequisites;

        public bool UserOwnsEquipment { get; } = queryResult.UserOwnsEquipment;
        public bool IsMinProgressionInRange { get; } = queryResult.IsMinProgressionInRange;
        public bool IsMaxProgressionInRange { get; } = queryResult.IsMaxProgressionInRange;
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;

        public bool AllCurrentVariationsIgnored { get; set; }
        public bool AllCurrentVariationsMissingEquipment { get; set; }
        public (string? name, string? reason) EasierVariation { get; set; }
        public (string? name, string? reason) HarderVariation { get; set; }
        public int? NextProgression { get; set; }

        public override int GetHashCode() => HashCode.Combine(Exercise.Id);
        public override bool Equals(object? obj) => obj is InProgressQueryResults other
            && other.Exercise.Id == Exercise.Id; // Group by exercises, not variations.
    }

    [DebuggerDisplay("{ExerciseId}: {VariationName}")]
    private class AllVariationsQueryResults(ExerciseVariationsQueryResults queryResult)
    {
        public int ExerciseId { get; } = queryResult.Exercise.Id;
        public int VariationId { get; } = queryResult.Variation.Id;
        public string VariationName { get; } = queryResult.Variation.Name;
        public Progression VariationProgression { get; } = queryResult.Variation.Progression;
        public bool IsIgnored { get; } = queryResult.UserVariation?.Ignore ?? false;
        public bool UserOwnsEquipment { get; } = queryResult.UserOwnsEquipment;
        public bool IsMinProgressionInRange { get; } = queryResult.IsMinProgressionInRange;
        public bool IsMaxProgressionInRange { get; } = queryResult.IsMaxProgressionInRange;
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;
    }

    [DebuggerDisplay("{ExerciseId}")]
    private class PrerequisitesQueryResults(ExerciseVariationsQueryResults queryResult)
    {
        public int ExerciseId { get; } = queryResult.Exercise.Id;
        public int UserExerciseProgression { get; } = queryResult.UserExercise?.Progression ?? UserConsts.MinUserProgression;
        public DateOnly UserExerciseLastSeen { get; } = queryResult.UserExercise?.LastSeen ?? DateOnly.MinValue;
        public DateOnly UserVariationLastSeen { get; } = queryResult.UserVariation?.LastSeen ?? DateOnly.MinValue;
        public Progression VariationProgression { get; } = queryResult.Variation.Progression;
    }

    public required UserOptions UserOptions { get; init; }
    public required SelectionOptions SelectionOptions { get; init; }
    public required ExclusionOptions ExclusionOptions { get; init; }
    public required ExerciseOptions ExerciseOptions { get; init; }
    public required MovementPatternOptions MovementPattern { get; init; }
    public required MuscleGroupOptions MuscleGroup { get; init; }
    public required SkillsOptions SkillsOptions { get; init; }
    public required SportsOptions SportsOptions { get; init; }
    public required EquipmentOptions EquipmentOptions { get; init; }
    public required ExerciseFocusOptions ExerciseFocusOptions { get; init; }
    public required MuscleMovementOptions MuscleMovementOptions { get; init; }

    private IQueryable<ExercisesQueryResults> CreateExercisesQuery(CoreContext context, bool includePrerequisites)
    {
        var query = context.Exercises.IgnoreQueryFilters().TagWith(nameof(CreateExercisesQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includePrerequisites)
        {
            return query.Select(i => new ExercisesQueryResults()
            {
                Exercise = i,
                UserExercise = i.UserExercises.First(ue => ue.UserId == UserOptions.Id),
                // Pull these out of the constructor so EF Core can filter out unused properties.
                Prerequisites = i.Prerequisites.Where(p => p.PrerequisiteExercise.DisabledReason == null).Select(p => new ExercisePrerequisiteDto()
                {
                    Proficiency = p.Proficiency,
                    Id = p.PrerequisiteExerciseId,
                    Name = p.PrerequisiteExercise.Name,
                }).ToList(),
                Postrequisites = i.Postrequisites.Where(p => p.Exercise.DisabledReason == null).Select(p => new ExercisePrerequisiteDto()
                {
                    Id = p.ExerciseId,
                    Name = p.Exercise.Name,
                    Proficiency = p.Proficiency,
                }).ToList()
            });
        }

        return query.Select(e => new ExercisesQueryResults()
        {
            Exercise = e,
            UserExercise = e.UserExercises.First(ue => ue.UserId == UserOptions.Id),
        });
    }

    private IQueryable<VariationsQueryResults> CreateVariationsQuery(CoreContext context, bool includeInstructions)
    {
        var query = context.Variations.IgnoreQueryFilters().TagWith(nameof(CreateVariationsQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includeInstructions)
        {
            query = query // Instruction equipment is auto included. Instruction location is auto included.
                .Include(v => v.Instructions.Where(d => d.DisabledReason == null).Where(eg => eg.Parent == null))
                    .ThenInclude(eg => eg.Children.Where(d => d.DisabledReason == null))
                        .ThenInclude(eg => eg.Children.Where(d => d.DisabledReason == null));
        }

        return query.Select(v => new VariationsQueryResults()
        {
            Variation = v,
            UserVariation = v.UserVariations.First(uv => uv.UserId == UserOptions.Id && uv.Section == section && section != Section.None)
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
                    o.Prerequisites,
                    o.Postrequisites,
                })
            .Select(a => new ExerciseVariationsQueryResults()
            {
                Exercise = a.Exercise,
                Variation = a.Variation,
                UserExercise = a.UserExercise,
                UserVariation = a.UserVariation,
                Prerequisites = a.Prerequisites,
                Postrequisites = a.Postrequisites,
                // Out of range when the exercise is too difficult for the user
                IsMinProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no minimum 
                    || a.Variation.Progression.Min == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise == null ? ((UserOptions.IsNewToFitness || Section.Rehab.HasFlag(section)) ? UserConsts.UserIsNewProgression : UserConsts.UserIsSeasonedProgression) : a.UserExercise.Progression) >= a.Variation.Progression.Min,
                // Out of range when the exercise is too easy for the user
                IsMaxProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no maximum
                    || a.Variation.Progression.Max == null
                    // Compare the exercise's progression range with the user's exercise progression
                    || (a.UserExercise == null ? ((UserOptions.IsNewToFitness || Section.Rehab.HasFlag(section)) ? UserConsts.UserIsNewProgression : UserConsts.UserIsSeasonedProgression) : a.UserExercise.Progression) < a.Variation.Progression.Max,
                // User owns at least one equipment in at least one of the optional equipment groups
                UserOwnsEquipment = UserOptions.NoUser
                    // There is an instruction that does not require any equipment
                    || a.Variation.DefaultInstruction != null
                    // Out of the instructions that require equipment, the user owns the equipment for the root instruction and the root instruction can be done on its own, or the user own the equipment of the child instructions. 
                    || a.Variation.Instructions.Where(i => i.Parent == null).Any(peg =>
                        // There is no equipment for the root instruction.
                        peg.Equipment == Equipment.None
                        // Or the user owns equipment for the root instruction.
                        || ((peg.Equipment & UserOptions.Equipment) != 0
                            // And the root instruction can be done on its own.
                            && (peg.Link != null
                                // Or the user owns the equipment for the child instructions or there is no equipment. HasAnyFlag
                                || peg.Children.Any(ceg => (ceg.Equipment & UserOptions.Equipment) != 0 || ceg.Equipment == Equipment.None)
                            )
                        )
                    )
            });
    }

    private IQueryable<ExerciseVariationsQueryResults> CreateFilteredExerciseVariationsQuery(CoreContext context, bool includeInstructions, bool includePrerequisites, bool ignoreExclusions = false)
    {
        var filteredQuery = CreateExerciseVariationsQuery(context,
                includeInstructions: includeInstructions,
                includePrerequisites: includePrerequisites)
            .TagWith(nameof(CreateFilteredExerciseVariationsQuery))
            // Filter down to variations the user owns equipment for.
            // If there are no Instructions and DefaultInstruction is null, the variation will be skipped.
            .Where(vm => UserOptions.IgnoreMissingEquipment || vm.UserOwnsEquipment)
            // Don't grab exercises that the user wants to ignore.
            .Where(vm => UserOptions.IgnoreIgnored || vm.UserExercise.Ignore != true)
            // Don't grab variations that the user wants to ignore.
            .Where(vm => UserOptions.IgnoreIgnored || vm.UserVariation.Ignore != true);

        if (!ignoreExclusions)
        {
            filteredQuery = filteredQuery
                // Don't grab exercises that we want to ignore.
                .Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id))
                // Don't grab variations that we want to ignore.
                .Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));

            // Don't grab skills that we want to ignore.
            foreach (var skillTypeSkill in ExclusionOptions.SkillTypeSkills)
            {
                filteredQuery = filteredQuery.Where(vm => skillTypeSkill.Key == vm.Exercise.SkillType && (skillTypeSkill.Value & vm.Exercise.Skills) == 0);
            }

            // Don't apply this to prerequisites.
            if (SelectionOptions.AllRefreshed)
            {
                // If AllRefreshed is true, further filter down to only variations that are due for refresh.
                // Otherwise, we'll order by the LastSeen date and choose the first, including the ones that have refresh padding.
                filteredQuery = filteredQuery.Where(vm => vm.UserVariation == null || (vm.UserVariation.LastSeen <= DateHelpers.Today && vm.UserVariation.RefreshAfter == null));
            }
        }

        // Apply this to prerequisites, so we never check against prerequisites the user cannot see.
        if (!UserOptions.NoUser && (UserOptions.IsNewToFitness || UserOptions.NeedsDeload))
        {
            // Don't show dangerous exercises when the user is new to fitness.
            filteredQuery = filteredQuery.Where(vm => !vm.Variation.UseCaution);
        }

        return filteredQuery;
    }

    /// <summary>
    /// Queries the db for the data.
    /// </summary>
    public async Task<IList<QueryResults>> Query(IServiceScopeFactory factory, int take = int.MaxValue)
    {
        // Short-circut when either of these options are set without any data. No results are returned.
        if (ExerciseOptions.ExerciseIds?.Any() == false || ExerciseOptions.VariationIds?.Any() == false)
        {
            return [];
        }

        using var scope = factory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var filteredQuery = CreateFilteredExerciseVariationsQuery(context,
            includePrerequisites: SelectionOptions.IncludePrerequisites,
            includeInstructions: SelectionOptions.IncludeInstructions);

        filteredQuery = Filters.FilterSection(filteredQuery, section);
        filteredQuery = Filters.FilterSkills(filteredQuery, SkillsOptions);
        filteredQuery = Filters.FilterEquipment(filteredQuery, EquipmentOptions.Equipment);
        filteredQuery = Filters.FilterSportsFocus(filteredQuery, SportsOptions.SportsFocus);
        filteredQuery = Filters.FilterExercises(filteredQuery, ExerciseOptions.ExerciseIds);
        filteredQuery = Filters.FilterVariations(filteredQuery, ExerciseOptions.VariationIds);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExerciseFocus);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPattern.MovementPatterns);
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExcludeExerciseFocus, exclude: true);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroup.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr2, n2) => curr2 | n2), include: true, MuscleGroup.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, UserOptions.ExcludeRecoveryMuscle, include: false, UserOptions.ExcludeRecoveryMuscleTarget);

        var queryResults = await filteredQuery.Select(a => new InProgressQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();

        var filteredResults = new List<InProgressQueryResults>();
        if (UserOptions.NoUser)
        {
            filteredResults = queryResults;
        }
        else
        {
            // Do this before querying prerequisites so that the user records also exist for the prerequisites.
            await AddMissingUserRecords(context, queryResults);

            // Grab a list of non-filtered variations for all the exercises we grabbed.
            // We only need exercise variations for the exercises in our query result set.
            var allExercisesVariations = await Filters.FilterExercises(CreateExerciseVariationsQuery(context, includeInstructions: false, includePrerequisites: false), queryResults.Select(qr => qr.Exercise.Id).ToList())
                .Select(a => new AllVariationsQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();

            var checkPrerequisitesFrom = new List<PrerequisitesQueryResults>();
            if (!UserOptions.IgnorePrerequisites)
            {
                // Grab a half-filtered list of exercises to check prerequisites against.
                // This filters down to only variations that the user owns equipment for.
                var checkPrerequisitesFromQuery = CreateFilteredExerciseVariationsQuery(context, includeInstructions: false, includePrerequisites: false, ignoreExclusions: true)
                    // Making sure the prerequisite has the potential to be seen by the user (within a recent timeframe).
                    // Checking this so we don't get stuck not seeing an exercise if the prerequisite can never be seen.
                    // FIXED: There is a small chance, since UserExercise records are created on the fly per section,
                    // ... that a prerequisite in another section won't apply until the next day.
                    // ... Happens mostly when building the user's very first newsletter.
                    // ... UserExercises from subsequent sections are yet to be made.
                    .Where(a => UserOptions.CreatedDate > DateHelpers.Today.AddMonths(-1) || a.UserExercise.LastVisible > DateHelpers.Today.AddMonths(-1));

                // We don't want to see a rehab exercise as a prerequisite when strength training.
                // We do want to see Planks and Dynamic Planks as a prerequisite for Mountain Climbers.
                // Only including exercises from the warmup, main, cooldown and the current section we are querying for,
                // ... so that that the rehab/prehab/sports sections will filter against their own section and the main three sections. 
                checkPrerequisitesFromQuery = Filters.FilterSection(checkPrerequisitesFromQuery, Section.Warmup | Section.Cooldown | Section.Main | section);

                // We don't check Depth Drops as a prerequisite for our exercise if that is a Basketball exercise and not a Soccer exercise.
                // IncludeNone is true so we check against exercises that our a part of the normal strength training (non-SportsFocus) regimen.
                checkPrerequisitesFromQuery = Filters.FilterSportsFocus(checkPrerequisitesFromQuery, SportsOptions.SportsFocus, includeNone: true);

                // Further filter down the exercises to those that match our query results.
                checkPrerequisitesFromQuery = Filters.FilterExercises(checkPrerequisitesFromQuery, queryResults.SelectMany(qr => qr.Prerequisites.Select(p => p.Id)).ToList());

                // Make sure we have a user before we query for prerequisites.
                checkPrerequisitesFrom = await checkPrerequisitesFromQuery.Select(a => new PrerequisitesQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
            }

            foreach (var queryResult in queryResults)
            {
                var queryResultExerciseVariations = allExercisesVariations.Where(ev => ev.ExerciseId == queryResult.Exercise.Id).ToList();
                
                // Grab variations that are in the user's progression range. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsIgnored = queryResultExerciseVariations.Count != 0 
                    && queryResultExerciseVariations.Where(ev => ev.IsProgressionInRange).All(ev => ev.IsIgnored);

                // Grab variations that the user owns the necessary equipment for. Use the non-filtered list when checking these so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsMissingEquipment = queryResultExerciseVariations.Count != 0
                    && queryResultExerciseVariations.Where(ev => ev.IsProgressionInRange).All(ev => !ev.UserOwnsEquipment);

                if (!UserOptions.IgnoreProgressions)
                {
                    queryResult.EasierVariation = (
                        queryResultExerciseVariations
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
                        queryResult.IsMinProgressionInRange ? null
                            : (queryResult.AllCurrentVariationsIgnored ? "Ignored"
                                : queryResult.AllCurrentVariationsMissingEquipment ? "Missing Equipment"
                                    // Likely the user changed progression levels and is viewing an old workout.
                                    : null) // ... or the variation has UseCaution set and the user is new to fitness.
                    );

                    queryResult.HarderVariation = (
                        queryResultExerciseVariations
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
                        queryResult.IsMaxProgressionInRange ? null
                            : (queryResult.AllCurrentVariationsIgnored ? "Ignored"
                                : queryResult.AllCurrentVariationsMissingEquipment ? "Missing Equipment"
                                    // Likely the user changed progression levels and is viewing an old workout.
                                    : null) // ... or the variation has UseCaution set and the user is new to fitness.
                    );

                    // The next variation in the exercise track based on variation progression levels
                    queryResult.NextProgression = queryResultExerciseVariations
                        // Stop at the lower bounds of variations    
                        // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                        .Where(ev => ev.IsIgnored == false)
                        .Select(ev => ev.VariationProgression.Min)
                        // Stop at the upper bounds of variations
                        .Union(queryResultExerciseVariations
                            // Don't include next progression that have been ignored, so that if the first two variations are ignored, we select the third
                            .Where(ev => ev.IsIgnored == false)
                            .Select(ev => ev.VariationProgression.Max)
                        )
                        .Where(mp => mp.HasValue && mp > queryResult.UserExercise!.Progression)
                        .OrderBy(mp => mp - queryResult.UserExercise!.Progression)
                        .FirstOrDefault();
                }

                if (!PrerequisitesPass(queryResult, checkPrerequisitesFrom))
                {
                    continue;
                }

                filteredResults.Add(queryResult);
            }

            if (!UserOptions.IgnoreProgressions)
            {
                // Try choosing variations that have a max progression above the user's progression.
                // Fallback to an easier variation if one does not exist. Group by exercises.
                filteredResults = filteredResults.GroupBy(i => i).SelectMany(g =>
                    // If there is no variation in the max user progression range (say, if the harder variation requires weights), take the next easiest variation.
                    g.Where(a => a.IsMinProgressionInRange && a.IsMaxProgressionInRange).NullIfEmpty()
                        // LINQ was not the way to go about this...
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
                            // FIXED: When filtering down to something like MovementPatterns, if the next highest
                            // ... variation that passes the MovementPattern filter is higher than the next highest
                            // ... variation that doesn't, then we will get a twice-as-difficult next variation.
                            .Where(a => a.Variation.Progression.MinOrDefault <= (g.Key.NextProgression ?? UserConsts.MaxUserProgression))
                            // FIXED: If two variations have the same min proficiency, should we select both? Yes
                            .GroupBy(e => e.Variation.Progression.MinOrDefault).OrderBy(k => k.Key).Take(1).SelectMany(k => k)
                ).ToList();
            }
        }

        // OrderBy must come after the query or you get cartesian explosion.
        var orderedResults = filteredResults
            // Order by variations that are still pending refresh.
            .OrderByDescending(a => a.UserVariation?.RefreshAfter.HasValue)
            // Show exercise variations that the user has rarely seen.
            // Adding the two in case there is a warmup and main variation in the same exercise.
            // ... Otherwise, since the warmup section is always chosen first, the last seen date is always updated and the main variation is rarely chosen.
            .ThenBy(a => a.UserExercise?.LastSeen.DayNumber + a.UserVariation?.LastSeen.DayNumber)
            // Mostly for the demo, show mostly random exercises.
            // NOTE: When the two variation's LastSeen dates are the same:
            // ... The LagRefreshXWeeks will prevent the LastSeen date from updating
            // ... and we may see two randomly alternating exercises for the LagRefreshXWeeks duration.
            .ThenBy(_ => RandomNumberGenerator.GetInt32(Int32.MaxValue))
            // Don't re-order the list on each read
            .ToList();

        var muscleTarget = MuscleGroup.MuscleTarget.Compile();
        var secondaryMuscleTarget = MuscleGroup.SecondaryMuscleTarget?.Compile();
        var finalResults = new List<QueryResults>();
        do
        {
            foreach (var exercise in orderedResults)
            {
                // Don't choose two variations of the same exercise.
                if (SelectionOptions.UniqueExercises && finalResults.Select(r => r.Exercise).Contains(exercise.Exercise))
                {
                    continue;
                }

                // Don't choose if there are any prerequisites already being worked.
                if (SelectionOptions.UniqueExercises && finalResults.Any(fr => exercise.Prerequisites.Any(ep => ep.Id == fr.Exercise.Id)))
                {
                    continue;
                }

                // Don't choose if there are any postrequisites already being worked.
                if (SelectionOptions.UniqueExercises && finalResults.Any(fr => exercise.Postrequisites.Any(ep => ep.Id == fr.Exercise.Id)))
                {
                    continue;
                }

                // Don't choose two variations that work the same skills.
                if (SelectionOptions.UniqueExercises && (finalResults.Aggregate(0, (curr, n) => curr | n.Exercise.Skills) & exercise.Exercise.Skills) != 0)
                {
                    continue;
                }

                // Don't overwork muscle groups.
                var overworkedMuscleGroups = GetOverworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);
                if (overworkedMuscleGroups.Any(mg => muscleTarget(exercise).HasAnyFlag(mg)))
                {
                    continue;
                }

                // Choose exercises that cover at least X muscles in the targeted muscles set.
                if (MuscleGroup.AtLeastXUniqueMusclesPerExercise.HasValue)
                {
                    var unworkedMuscleGroups = GetUnworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);

                    // We've already worked all unique muscles.
                    if (unworkedMuscleGroups.Count == 0)
                    {
                        break;
                    }

                    // Find the number of weeks of padding that this variation still has left. If the padded refresh date is earlier than today, then use the number 0.
                    var weeksTillLastSeen = Math.Max(0, (exercise.UserVariation?.LastSeen.DayNumber ?? DateHelpers.Today.DayNumber) - DateHelpers.Today.DayNumber) / 7;
                    // Allow exercises that have a refresh date since we want to show those continuously until that date.
                    // Allow the first exercise with any muscle group so the user does not get stuck from seeing certain exercises
                    // ... if, for example, a prerequisite only works one muscle group and that muscle group is otherwise worked by compound exercises.
                    var musclesToWork = (exercise.UserVariation?.RefreshAfter != null || !finalResults.Any(e => e.UserVariation?.RefreshAfter == null)) ? 1
                        // Choose two variations with no refresh padding and few muscles worked over a variation with lots of refresh padding and many muscles worked.
                        // Doing weeks out so we still prefer variations with many muscles worked to an extent.
                        : (MuscleGroup.AtLeastXUniqueMusclesPerExercise.Value + weeksTillLastSeen);

                    // The exercise does not work enough unique muscles that we are trying to target.
                    if (unworkedMuscleGroups.Count(mg => muscleTarget(exercise).HasAnyFlag(mg)) < musclesToWork)
                    {
                        continue;
                    }
                }

                // Choose exercises that cover a unique movement pattern.
                if (MovementPattern.MovementPatterns.HasValue && MovementPattern.IsUnique)
                {
                    var unworkedMovementPatterns = EnumExtensions.GetValuesExcluding(Core.Models.Exercise.MovementPattern.None, Core.Models.Exercise.MovementPattern.All)
                        // The movement pattern is in our list of movement patterns to work.
                        .Where(v => MovementPattern.MovementPatterns.Value.HasFlag(v))
                        // The movement pattern has not yet been worked.
                        .Where(mp => !finalResults.Any(r => mp.HasAnyFlag(r.Variation.MovementPattern)));

                    // We've already worked all unique movement patterns.
                    if (!unworkedMovementPatterns.Any())
                    {
                        break;
                    }

                    // If none of the unworked movement patterns match up with the variation's movement patterns.
                    if (!unworkedMovementPatterns.Any(mp => mp.HasAnyFlag(exercise.Variation.MovementPattern)))
                    {
                        continue;
                    }
                }

                finalResults.Add(new QueryResults(section, exercise.Exercise, exercise.Variation, exercise.UserExercise, exercise.UserVariation, exercise.Prerequisites, exercise.Postrequisites, exercise.EasierVariation, exercise.HarderVariation, UserOptions.Intensity));
            }
        }
        // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
        // Choose a 3-muscle group compound exercise or a 2-muscle group compound exercise and then an isolation exercise.
        while (MuscleGroup.AtLeastXUniqueMusclesPerExercise.HasValue && --MuscleGroup.AtLeastXUniqueMusclesPerExercise >= 1);

        // REFACTORME
        return (UserOptions.NoUser, section) switch
        {
            (true, _) or (_, Section.None) => [
                .. finalResults.Take(take)
                    // Not in a workout context, order by progression levels.
                    .OrderBy(vm => vm.Variation.Progression.Min)
                    .ThenBy(vm => vm.Variation.Progression.Max == null)
                    .ThenBy(vm => vm.Variation.Progression.Max)
                    .ThenBy(vm => vm.Variation.Name)
            ],
            (_, Section.Debug) => [
                .. finalResults
                    // Not in a workout context, order by progression levels.
                    .GroupBy(vm => vm.Exercise)
                    .Take(take)
                    .SelectMany(vm => vm)
                    .OrderBy(vm => vm.Variation.Progression.Min)
                    .ThenBy(vm => vm.Variation.Progression.Max == null)
                    .ThenBy(vm => vm.Variation.Progression.Max)
                    .ThenBy(vm => vm.Variation.Name)
            ],
            (_, Section.WarmupRaise) => [
                .. finalResults.Take(take)
                    // Order by least expected difficulty first.
                    .OrderBy(vm => muscleTarget(vm).PopCount())
            ],
            (_, Section.Core) => [
                .. finalResults.Take(take)
                    // Show exercises that work a muscle target we want more of first.
                    .OrderByDescending(vm => (muscleTarget(vm) & MuscleGroup.AllMuscleGroups).PopCount())
                    // Then by hardest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            (_, Section.Accessory) => [
                .. finalResults.Take(take)
                    // Core exercises last.
                    .OrderBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then by hardest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            (_, Section.Functional) => [
                .. finalResults.Take(take)
                    // Plyometrics first.
                    .OrderByDescending(vm => vm.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Speed))
                    // Core exercises last.
                    .ThenBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then by hardest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            _ => finalResults.Take(take).ToList() // We are in a workout context, keep the result order.
        };
    }

    private static bool PrerequisitesPass(InProgressQueryResults queryResult, IList<PrerequisitesQueryResults> checkPrerequisitesFrom)
    {
        foreach (var prerequisiteToCheck in checkPrerequisitesFrom)
        {
            // The prerequisite is in the list of filtered exercises, so that we don't see a rehab exercise as a prerequisite when strength training.
            var prerequisiteProficiency = queryResult.Prerequisites.FirstOrDefault(p => p.Id == prerequisiteToCheck.ExerciseId)?.Proficiency;
            if (prerequisiteProficiency == null)
            {
                continue;
            }

            // The prerequisite falls in the range of the exercise's proficiency level.
            if (prerequisiteProficiency >= prerequisiteToCheck.VariationProgression.MinOrDefault
                && prerequisiteProficiency < prerequisiteToCheck.VariationProgression.MaxOrDefault)
            {
                // User is at the required proficiency level.
                if (prerequisiteToCheck.UserExerciseProgression == prerequisiteProficiency
                    // And the prerequisite exercise has been seen in the past.
                    // ... We don't want to show Handstand Pushups before the user has seen Pushups.
                    && prerequisiteToCheck.UserExerciseLastSeen > DateOnly.MinValue
                    // And all of the prerequisite's proficiency variations have been seen in the past.
                    // ... We don't want to show Handstand Pushups before the user has seen Full Pushups.
                    && prerequisiteToCheck.UserVariationLastSeen > DateOnly.MinValue)
                {
                    continue;
                }

                // User is past the required proficiency level.
                if (prerequisiteToCheck.UserExerciseProgression > prerequisiteProficiency
                    // The prerequisite exercise has been seen in the past.
                    // We don't want to show Handstand Pushups before the user has seen Pushups.
                    && prerequisiteToCheck.UserExerciseLastSeen > DateOnly.MinValue)
                {
                    continue;
                }

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
        if (section == Section.None) { return; }

        // Check this first so that the LastVisible date is not updated immediately after the UserExercise record is created.
        var exercisesUpdated = new HashSet<UserExercise>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserExercise != null))
        {
            queryResult.UserExercise!.LastVisible = DateHelpers.Today;
            if (exercisesUpdated.Add(queryResult.UserExercise))
            {
                context.UserExercises.Update(queryResult.UserExercise);
            }
        }

        var exercisesCreated = new HashSet<UserExercise>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserExercise == null))
        {
            queryResult.UserExercise = new UserExercise()
            {
                UserId = UserOptions.Id,
                ExerciseId = queryResult.Exercise.Id,
                // If the user is new to fitness or if the section is a rehab section, start at the min progression level. Otherwise, start at mid progression level.
                Progression = (UserOptions.IsNewToFitness || Section.Rehab.HasFlag(section)) ? UserConsts.UserIsNewProgression : UserConsts.UserIsSeasonedProgression
            };

            if (exercisesCreated.Add(queryResult.UserExercise))
            {
                context.UserExercises.Add(queryResult.UserExercise);
            }
        }

        var variationsCreated = new HashSet<UserVariation>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserVariation == null))
        {
            queryResult.UserVariation = new UserVariation()
            {
                VariationId = queryResult.Variation.Id,
                UserId = UserOptions.Id,
                Section = section
            };

            if (variationsCreated.Add(queryResult.UserVariation))
            {
                context.UserVariations.Add(queryResult.UserVariation);
            }
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException e) when (e.IsDuplicateKeyException())
        {
            // Ignoring duplicate key exceptions since the entities are set on the queryResult either way.
            // See if EF Core implements ON CONFLICT IGNORE or ON CONFLICT UPDATE in the future.
        }
    }

    private List<MusculoskeletalSystem> GetUnworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        return MuscleGroup.MuscleTargetsRDA.Where(kv =>
        {
            // We are targeting this muscle group.
            var workedCount = finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have not overworked this muscle group and this muscle group is a part of our worked set.
            return workedCount < kv.Value && MuscleGroup.AllMuscleGroups.HasFlag(kv.Key);
        }).Select(kv => kv.Key).ToList();
    }

    private List<MusculoskeletalSystem> GetOverworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        // Not checking if this muscle group is a part of our worked set.
        // We don't want to overwork any muscle regardless if we are targeting it.
        return MuscleGroup.MuscleTargetsTUL.Where(kv =>
        {
            var workedCount = finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleCount(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have overworked this muscle group.
            return workedCount > kv.Value;
        }).Select(kv => kv.Key).ToList();
    }
}
