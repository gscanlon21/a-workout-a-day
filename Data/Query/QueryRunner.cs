using Core.Dtos.Exercise;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Code.Extensions;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Query.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Security.Cryptography;
using static Core.Code.Extensions.EnumerableExtensions;

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

        public bool IsMinProgressionInRange { get; } = queryResult.IsMinProgressionInRange;
        public bool IsMaxProgressionInRange { get; } = queryResult.IsMaxProgressionInRange;
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;

        public bool AllCurrentVariationsIgnored { get; set; }
        public bool AllCurrentVariationsInvisible { get; set; }
        public bool AllCurrentVariationsDangerous { get; set; }
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
        public bool UseCaution { get; } = queryResult.Variation.UseCaution;
        public bool UserOwnsEquipment { get; } = queryResult.UserOwnsEquipment;
        public bool IsIgnored { get; } = queryResult.UserVariation?.Ignore ?? false;
        public bool IsMinProgressionInRange { get; } = queryResult.IsMinProgressionInRange;
        public bool IsMaxProgressionInRange { get; } = queryResult.IsMaxProgressionInRange;
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;
        public Progression VariationProgression { get; } = queryResult.Variation.Progression;
        public DateOnly? LastVisible { get; } = queryResult.UserExercise?.LastVisible;
    }

    [DebuggerDisplay("{ExerciseId}: {VariationProgression}")]
    private class PrerequisitesQueryResults(ExerciseVariationsQueryResults queryResult)
    {
        public int ExerciseId { get; } = queryResult.Exercise.Id;

        public DateOnly? UserExerciseLastSeen { get; } = queryResult.UserExercise?.LastSeen;
        public DateOnly? UserExerciseFirstSeen { get; } = queryResult.UserExercise?.FirstSeen;
        public DateOnly? UserVariationLastSeen { get; } = queryResult.UserVariation?.LastSeen;
        public DateOnly? UserVariationFirstSeen { get; } = queryResult.UserVariation?.FirstSeen;

        public Progression VariationProgression { get; } = queryResult.Variation.Progression;
        public int UserExerciseProgression { get; } = queryResult.UserExercise?.Progression ?? UserConsts.UserProgressionMin;
    }

    public required UserOptions UserOptions { get; init; }
    public required SkillsOptions SkillsOptions { get; init; }
    public required SportsOptions SportsOptions { get; init; }
    public required ExerciseOptions ExerciseOptions { get; init; }
    public required EquipmentOptions EquipmentOptions { get; init; }
    public required ExclusionOptions ExclusionOptions { get; init; }
    public required SelectionOptions SelectionOptions { get; init; }
    public required MuscleGroupOptions MuscleGroupOptions { get; init; }
    public required ExerciseFocusOptions ExerciseFocusOptions { get; init; }
    public required MuscleMovementOptions MuscleMovementOptions { get; init; }
    public required MovementPatternOptions MovementPatternOptions { get; init; }
    private DateOnly StaleBeforeDate { get; } = DateHelpers.Today.AddDays(-ExerciseConsts.StaleAfterDays);

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
            query = query // Include root instructions and then include those root instruction's child instructions.
                .Include(v => v.Instructions.Where(c => c.DisabledReason == null).Where(i => i.Parent == null))
                    .ThenInclude(i => i.Children.Where(c => c.DisabledReason == null)) // And child's children.
                        .ThenInclude(i => i.Children.Where(c => c.DisabledReason == null));
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
            .Select(ev => new ExerciseVariationsQueryResults()
            {
                Exercise = ev.Exercise,
                Variation = ev.Variation,
                UserExercise = ev.UserExercise,
                UserVariation = ev.UserVariation,
                Prerequisites = ev.Prerequisites,
                Postrequisites = ev.Postrequisites,
                // Out of range when the exercise is too difficult for the user.
                IsMinProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no minimum.
                    || ev.Variation.Progression.Min == null
                    // Compare the exercise's progression range with the user's exercise progression.
                    // When building the first workout this will only return true for variations w/o progression ranges,
                    // ... but that shouldn't matter with the backfill. After UserExercise records are created it works properly.
                    // ... Not adding in default values because that complicates later changes if we change starting progressions.
                    || ev.UserExercise.Progression >= ev.Variation.Progression.Min,
                // Out of range when the exercise is too easy for the user.
                IsMaxProgressionInRange = UserOptions.NoUser
                    // This exercise variation has no maximum.
                    || ev.Variation.Progression.Max == null
                    // Compare the exercise's progression range with the user's exercise progression.
                    // When building the first workout this will only return true for variations w/o progression ranges,
                    // ... but that shouldn't matter with the backfill. After UserExercise records are created it works properly.
                    // ... Not adding in default values because that complicates later changes if we change starting progressions.
                    || ev.UserExercise.Progression < ev.Variation.Progression.Max,
                // User owns at least one equipment in at least one of the optional equipment groups.
                // If there are no Instructions and DefaultInstruction is null, then the variation will be skipped.
                UserOwnsEquipment = UserOptions.NoUser
                    // There is an instruction that does not require any equipment.
                    || ev.Variation.DefaultInstruction != null
                    // Out of the instructions that require equipment: the user owns the equipment for:
                    // ... the root instruction and the root instruction can be done on its own,
                    // ... or the user own the equipment for the child instructions. 
                    || ev.Variation.Instructions.Where(i => i.Parent == null).Any(peg =>
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
            .TagWith(nameof(CreateFilteredExerciseVariationsQuery));

        // Not in a workout context, ignore user filtering.
        if (!UserOptions.NoUser && section != Section.None)
        {
            // Filter down to variations the user owns equipment for.
            filteredQuery = filteredQuery.Where(vm => vm.UserOwnsEquipment)
                // Don't grab variations that the user wants to ignore.
                .Where(vm => vm.UserVariation.Ignore != true)
                // Don't grab exercises that the user wants to ignore.
                .Where(vm => vm.UserExercise.Ignore != true);
        }

        // Don't apply these to prerequisites.
        if (!ignoreExclusions)
        {
            // Don't grab exercises that we want to ignore.
            if (ExclusionOptions.ExerciseIds.Any())
            {
                filteredQuery = filteredQuery.Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id));
            }

            // Don't grab variations that we want to ignore.
            if (ExclusionOptions.VariationIds.Any())
            {
                filteredQuery = filteredQuery.Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.VocalSkills != VocalSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                filteredQuery = filteredQuery.Where(vm => vm.Exercise.VocalSkills == 0 || (ExclusionOptions.VocalSkills & vm.Exercise.VocalSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.VisualSkills != VisualSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                filteredQuery = filteredQuery.Where(vm => vm.Exercise.VisualSkills == 0 || (ExclusionOptions.VisualSkills & vm.Exercise.VisualSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.CervicalSkills != CervicalSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                filteredQuery = filteredQuery.Where(vm => vm.Exercise.CervicalSkills == 0 || (ExclusionOptions.CervicalSkills & vm.Exercise.CervicalSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.ThoracicSkills != ThoracicSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                filteredQuery = filteredQuery.Where(vm => vm.Exercise.ThoracicSkills == 0 || (ExclusionOptions.ThoracicSkills & vm.Exercise.ThoracicSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.LumbarSkills != LumbarSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                filteredQuery = filteredQuery.Where(vm => vm.Exercise.LumbarSkills == 0 || (ExclusionOptions.LumbarSkills & vm.Exercise.LumbarSkills) == 0);
            }

            // Filter out padded refresh variations.
            if (SelectionOptions.AllRefreshed)
            {
                // Include lagged refresh variations (RefreshAfter != null), so they always show up while pending refresh.
                filteredQuery = filteredQuery.Where(vm => vm.UserVariation.LastSeen == null || vm.UserVariation.LastSeen <= DateHelpers.Today);
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
    /// <param name="take">Selects this many variations.</param>
    public async Task<IList<QueryResults>> Query(IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
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
        filteredQuery = Filters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);
        filteredQuery = Filters.FilterMovementPattern(filteredQuery, MovementPatternOptions.MovementPatterns);
        filteredQuery = Filters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExcludeExerciseFocus, exclude: true);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, MuscleGroupOptions.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr2, n2) => curr2 | n2), include: true, MuscleGroupOptions.MuscleTarget);
        filteredQuery = Filters.FilterMuscleGroup(filteredQuery, UserOptions.ExcludeRecoveryMuscle, include: false, UserOptions.ExcludeRecoveryMuscleTarget);

        var queryResults = await filteredQuery.Select(a => new InProgressQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();

        // When you perform comparisons with nullable types, if the value of one of the nullable types
        // ... is null and the other is not, all comparisons evaluate to false except for != (not equal).
        var filteredResults = new List<InProgressQueryResults>();
        if (UserOptions.NoUser || section == Section.None)
        {
            filteredResults = queryResults;
        }
        else
        {
            // Do this before querying prerequisites so that the user records also exist for the prerequisites.
            await AddMissingUserRecords(context, queryResults);

            // Grab a list of non-filtered variations for all the exercises we grabbed.
            var allExercisesVariations = await GetAllExercisesVariations(context, queryResults);

            // Grab a list of prerequisite exercises that we can check user progressions for.
            var checkPrerequisitesFrom = await GetPrerequisites(context, queryResults);

            foreach (var queryResult in queryResults)
            {
                var queryResultExerciseVariations = allExercisesVariations.Where(ev => ev.ExerciseId == queryResult.Exercise.Id).ToList();
                var queryResultExerciseVariationsInRange = queryResultExerciseVariations.Where(ev => ev.IsProgressionInRange).ToList();

                // Check if all variations in the user's progression range have been ignored by the user.
                // Use the non-filtered list so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsIgnored = queryResultExerciseVariationsInRange.AllIfAny(ev => ev.IsIgnored);

                // Check if all variations in the user's progression range are not being seen by the user.
                // Use the non-filtered list so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsInvisible = queryResultExerciseVariationsInRange.AllIfAny(ev => ev.LastVisible < StaleBeforeDate);

                // Check if all variations in the user's progression range have been ignored by the user.
                // Use the non-filtered list so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsDangerous = queryResultExerciseVariationsInRange.AllIfAny(ev => ev.UseCaution && UserOptions.IsNewToFitness);

                // Check if all variations in the user's progression range are missing required equipment.
                // Use the non-filtered list so we can see if we need to grab an out-of-range progression.
                queryResult.AllCurrentVariationsMissingEquipment = queryResultExerciseVariationsInRange.AllIfAny(ev => !ev.UserOwnsEquipment);

                // This is required for the main and old workouts.
                // The old workout passes in IgnoreProgression:true,
                // ... so we can't check that to skip processing these.
                queryResult.EasierVariation = (
                    queryResultExerciseVariations
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderByDescending(ev => ev.VariationProgression.Max)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation.
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.IsIgnored == true : ev.IsIgnored == false)
                        .FirstOrDefault(ev => ev.VariationId != queryResult.Variation.Id
                            && (// UserExercise may be null when looking at prerequisites/postrequisites of an early newsletter's exercise.
                                // Current progression is in range, choose the previous progression by looking at the user's current progression level. If null is always false.
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.VariationProgression.Max <= queryResult.UserExercise?.Progression)
                                // Current progression is out of range, choose the previous progression by looking at current exercise's min progression.
                                || (!queryResult.IsMinProgressionInRange && ev.VariationProgression.Max <= queryResult.Variation.Progression.Min)
                            ))?
                        .VariationName,
                    queryResult.IsMinProgressionInRange ? null
                        : (queryResult.AllCurrentVariationsIgnored ? "Ignored"
                            : (queryResult.AllCurrentVariationsMissingEquipment ? "Missing Equipment"
                                : (queryResult.AllCurrentVariationsDangerous ? "Caution+NewToFitness"
                                    // Likely the user changed progression levels and is viewing an old workout.
                                    : null))) // ... or the variation has UseCaution set and the user is new to fitness.
                );

                // This is required for the main and old workouts.
                // The old workout passes in IgnoreProgression:true,
                // ... so we can't check that to skip processing these.
                queryResult.HarderVariation = (
                    queryResultExerciseVariations
                        // Don't show ignored variations? (untested)
                        //.Where(ev => ev.Variation.UserVariations.FirstOrDefault(uv => uv.User == User)!.Ignore != true)
                        .OrderBy(ev => ev.VariationProgression.Min)
                        // Choose the variation that is ignored if all the current variations are ignored, otherwise choose the un-ignored variation
                        .ThenBy(ev => queryResult.AllCurrentVariationsIgnored ? ev.IsIgnored == true : ev.IsIgnored == false)
                        .FirstOrDefault(ev => ev.VariationId != queryResult.Variation.Id
                            && (// UserExercise may be null when looking at prerequisites/postrequisites of an early newsletter's exercise.
                                // Current progression is in range, choose the next progression by looking at the user's current progression level. If null is always false.
                                (queryResult.IsMinProgressionInRange && queryResult.IsMaxProgressionInRange && ev.VariationProgression.Min > queryResult.UserExercise?.Progression)
                                // Current progression is out of range, choose the next progression by looking at current exercise's min progression.
                                || (!queryResult.IsMaxProgressionInRange && ev.VariationProgression.Min >= queryResult.Variation.Progression.Max)
                            ))?
                        .VariationName,
                   queryResult.IsMaxProgressionInRange ? null
                        : (queryResult.AllCurrentVariationsIgnored ? "Ignored"
                            : (queryResult.AllCurrentVariationsMissingEquipment ? "Missing Equipment"
                                : (queryResult.AllCurrentVariationsDangerous ? "Caution+NewToFitness"
                                    // Likely the user changed progression levels and is viewing an old workout.
                                    : null))) // ... or the variation has UseCaution set and the user is new to fitness.
                );

                if (!UserOptions.IgnoreProgressions)
                {
                    // The next progression level stop in the exercise track.
                    queryResult.NextProgression = queryResultExerciseVariations
                        // Stop at the lower bounds of variations.
                        // Skip ignored progressions levels.
                        .Where(ev => ev.IsIgnored == false)
                        .Select(ev => ev.VariationProgression.Min)
                        // Stop at the upper bounds of variations.
                        .Union(queryResultExerciseVariations
                            // Skip ignored progressions levels.
                            .Where(ev => ev.IsIgnored == false)
                            .Select(ev => ev.VariationProgression.Max)
                        )// If either int? is null, it is always false.
                        .Where(mp => mp > queryResult.UserExercise?.Progression)
                        .OrderBy(mp => mp - queryResult.UserExercise!.Progression)
                        .FirstOrDefault();
                }

                // If IgnorePrerequisites, this is skipped b/c the prereq list is empty.
                if (!PrerequisitesPass(queryResult, checkPrerequisitesFrom))
                {
                    continue;
                }

                filteredResults.Add(queryResult);
            }

            if (!UserOptions.IgnoreProgressions)
            {
                // Try choosing variations that are in the user's exercise progression range.
                // Fallback to an easier variation if one does not exist. Group by exercises.
                filteredResults = filteredResults.GroupBy(i => i).SelectMany(g =>
                    // LINQ was definitely not the way to go about this...    
                    g.Where(a => a.IsProgressionInRange).NullIfEmpty()
                        // If there is no progression in range, try to take the next easiest variation.
                        ?? g.Where(a => !a.IsMaxProgressionInRange /*&& Proficiency.AllowLesserProgressions*/)
                            // Only grab lower progressions when all of the current variations are ignored.
                            // It's possible a lack of equipment causes the current variation to not show.
                            .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment || a.AllCurrentVariationsInvisible || a.AllCurrentVariationsDangerous)
                            // If two variations of an exercise have the same max proficiency, then select both. Order by hardest and select the first.
                            .GroupBy(e => e.Variation.Progression.MaxOrDefault).OrderByDescending(g => g.Key).Take(1).SelectMany(g => g).NullIfEmpty()
                        // If there is no lesser progression, try to take the next harder variation.
                        // We do this so the user doesn't get stuck at the beginning of an exercise track
                        // ... if they ignore the first variation instead of progressing through the track.
                        ?? g.Where(a => !a.IsMinProgressionInRange /*&& Proficiency.AllowGreaterProgressions*/)
                            // Only grab higher progressions when all of the current variations are ignored.
                            // It's possible a lack of equipment causes the current variation to not show.
                            .Where(a => a.AllCurrentVariationsIgnored || a.AllCurrentVariationsMissingEquipment || a.AllCurrentVariationsInvisible || a.AllCurrentVariationsDangerous)
                            // FIXED: When filtering down to something like MovementPatterns, if the next highest variation that passes the MovementPattern
                            // ... filter is higher than the next highest variation that doesn't, then we will get a twice-as-difficult next variation.
                            .Where(a => a.Variation.Progression.MinOrDefault <= (g.Key.NextProgression ?? UserConsts.UserProgressionMax))
                            // If two variations have the same min proficiency, then select both. Order by easiest and select the first.
                            .GroupBy(e => e.Variation.Progression.MinOrDefault).OrderBy(g => g.Key).Take(1).SelectMany(g => g)
                ).ToList();
            }
        }

        // OrderBy must come after the query or you get cartesian explosion.
        List<InProgressQueryResults> orderedResults;
        if (SelectionOptions.Randomized)
        {
            // Randomize the order. Useful for the backfill because those workouts don't update the last seen date.
            orderedResults = filteredResults.OrderBy(_ => RandomNumberGenerator.GetInt32(Int32.MaxValue)).ToList();
        }
        else
        {
            // Variations that have a refresh delay should be ordered first.
            orderedResults = filteredResults.OrderByDescending(a => a.UserVariation?.RefreshAfter.HasValue, NullOrder.NullsLast)
                // Then show exercise variations that the user has least recently seen.
                // Adding the two in case there is a warmup and main variation in the same exercise.
                // ... Otherwise, since the warmup section is always chosen first, the last seen date is always updated and the main variation is rarely chosen.
                .ThenBy(a => a.UserExercise?.LastSeen?.DayNumber + a.UserVariation?.LastSeen?.DayNumber, NullOrder.NullsFirst)
                // Mostly for the demo, show mostly random exercises.
                // TODO? Order by the number of postrequisites descending?
                // NOTE: When the two variation's LastSeen dates are the same:
                // ... The LagRefreshXWeeks will prevent the LastSeen date from updating
                // ... and we may see two randomly alternating exercises for the LagRefreshXWeeks duration.
                .ThenBy(_ => RandomNumberGenerator.GetInt32(Int32.MaxValue))
                // Don't re-order the list on each read.
                .ToList();
        }

        var muscleTarget = MuscleGroupOptions.MuscleTarget.Compile();
        var secondaryMuscleTarget = MuscleGroupOptions.SecondaryMuscleTarget?.Compile();
        var finalResults = new List<QueryResults>();
        do
        {
            foreach (var exercise in orderedResults)
            {
                // Use this to add a tad more variety.
                if (SelectionOptions.UniqueExercises)
                {
                    var finalResultsExerciseIds = finalResults.Select(fr => fr.Exercise.Id).ToList();

                    // Don't choose two variations of the same exercise.
                    if (finalResultsExerciseIds.Contains(exercise.Exercise.Id))
                    {
                        continue;
                    }

                    // Don't choose if all prerequisites are being worked. 
                    if (exercise.Prerequisites.AllIfAny(p => finalResultsExerciseIds.Contains(p.Id) || ExclusionOptions.ExerciseIds.Contains(p.Id)))
                    {
                        continue;
                    }

                    // Don't choose if all postrequisites are being worked.
                    if (exercise.Postrequisites.AllIfAny(p => finalResultsExerciseIds.Contains(p.Id) || ExclusionOptions.ExerciseIds.Contains(p.Id)))
                    {
                        continue;
                    }

                    // If the exercise has skills.
                    if (exercise.Exercise.VocalSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                        && (finalResults.Aggregate(VocalSkills.None, (c, n) => c | n.Exercise.VocalSkills) & exercise.Exercise.VocalSkills) == exercise.Exercise.VocalSkills)
                    {
                        continue;
                    }

                    // If the exercise has skills.
                    if (exercise.Exercise.VisualSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                        && (finalResults.Aggregate(VisualSkills.None, (c, n) => c | n.Exercise.VisualSkills) & exercise.Exercise.VisualSkills) == exercise.Exercise.VisualSkills)
                    {
                        continue;
                    }

                    // If the exercise has skills.
                    if (exercise.Exercise.CervicalSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                        && (finalResults.Aggregate(CervicalSkills.None, (c, n) => c | n.Exercise.CervicalSkills) & exercise.Exercise.CervicalSkills) == exercise.Exercise.CervicalSkills)
                    {
                        continue;
                    }

                    // If the exercise has skills.                
                    if (exercise.Exercise.ThoracicSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                        && (finalResults.Aggregate(ThoracicSkills.None, (c, n) => c | n.Exercise.ThoracicSkills) & exercise.Exercise.ThoracicSkills) == exercise.Exercise.ThoracicSkills)
                    {
                        continue;
                    }

                    // If the exercise has skills.
                    if (exercise.Exercise.LumbarSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                        && (finalResults.Aggregate(LumbarSkills.None, (c, n) => c | n.Exercise.LumbarSkills) & exercise.Exercise.LumbarSkills) == exercise.Exercise.LumbarSkills)
                    {
                        continue;
                    }
                }

                // Choose exercises that cover a unique movement pattern.
                if (MovementPatternOptions.MovementPatterns.HasValue && MovementPatternOptions.IsUnique)
                {
                    var unworkedMovementPatterns = EnumExtensions.GetValuesExcluding(MovementPattern.None, MovementPattern.All)
                        // The movement pattern has not yet been worked. Checking any flag so we don't double up.
                        .Where(mp => !finalResults.Any(r => mp.HasAnyFlag(r.Variation.MovementPattern)))
                        // The movement pattern is in our list of movement patterns to select from.
                        .Where(v => MovementPatternOptions.MovementPatterns.Value.HasFlag(v));

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

                // Choose exercises that cover at least X muscles in our targeted muscles set.
                if (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.HasValue)
                {
                    var unworkedMuscleGroups = GetUnworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);

                    // We've already worked all unique muscles.
                    if (unworkedMuscleGroups.Count == 0)
                    {
                        break;
                    }

                    // Find the number of weeks of padding that this variation still has left. If the padded refresh date is earlier than today, then use the number 0.
                    var weeksFromLastSeen = Math.Max(0, (exercise.UserVariation?.LastSeen?.DayNumber ?? DateHelpers.Today.DayNumber) - DateHelpers.Today.DayNumber) / 7;
                    // Allow exercises that have a refresh date since we want to show those continuously until that date.
                    // Allow the first exercise with any muscle group so the user does not get stuck from seeing certain exercises
                    // ... if, for example, a prerequisite only works one muscle group and that muscle group is otherwise worked by compound exercises.
                    var musclesToWork = (exercise.UserVariation?.RefreshAfter != null || !finalResults.Any(e => e.UserVariation?.RefreshAfter == null)) ? 1
                        // Choose two variations with no refresh padding and few muscles worked over a variation with lots of refresh padding and many muscles worked.
                        // Doing weeks out so we still prefer variations with many muscles worked to an extent.
                        : (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.Value + weeksFromLastSeen);

                    // The exercise does not work enough unique muscles that we are trying to target.
                    if (unworkedMuscleGroups.Count(mg => muscleTarget(exercise).HasAnyFlag(mg)) < musclesToWork)
                    {
                        continue;
                    }
                }

                // Don't overwork muscle groups. Run this after MuscleGroups/MovementPatterns so we can break early if there are no muscle groups left to work.
                var overworkedMuscleGroups = GetOverworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);
                if (overworkedMuscleGroups.Any(mg => muscleTarget(exercise).HasAnyFlag(mg)))
                {
                    continue;
                }

                finalResults.Add(new QueryResults(section, exercise.Exercise, exercise.Variation, exercise.UserExercise, exercise.UserVariation, exercise.Prerequisites, exercise.Postrequisites, exercise.EasierVariation, exercise.HarderVariation, UserOptions.Intensity));
                if (finalResults.Count >= take)
                {
                    break;
                }
            }
        }
        // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
        // Choose a 3-muscle group compound exercise and then choose a 2-muscle group compound exercise and then choose an isolation exercise.
        while (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.HasValue && --MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise >= 1);

        return orderBy switch
        {
            OrderBy.ProgressionLevels => [
                // Not in a workout context, order by progression levels.
                .. finalResults.OrderBy(vm => vm.Variation.Progression.Min)
                    .ThenBy(vm => vm.Variation.Progression.Max == null)
                    .ThenBy(vm => vm.Variation.Progression.Max)
                    .ThenBy(vm => vm.Variation.Name)
            ],
            OrderBy.LeastDifficultFirst => [
                // Order by least expected difficulty first.
                .. finalResults.OrderBy(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.MusclesTargeted => [
                // Show exercises that work a muscle target we want more of first.
                .. finalResults.OrderByDescending(vm => (muscleTarget(vm) & MuscleGroupOptions.AllMuscleGroups).PopCount())
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.CoreLast => [
                // Core exercises last.
                .. finalResults.OrderBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.PlyometricsFirst => [
                // Order plyometrics first. They're best done early in the workout when the user isn't fatigued.
                .. finalResults.OrderByDescending(vm => vm.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Speed))
                    // Core exercises last. Ordering exercises that don't work core muscles first.
                    .ThenBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            _ => finalResults.ToList() // We are in a workout context, keep the result order.
        };
    }

    /// <summary>
    /// Grab a list of non-filtered variations for all the exercises we grabbed.
    /// </summary>
    private async Task<List<AllVariationsQueryResults>> GetAllExercisesVariations(CoreContext context, IList<InProgressQueryResults> queryResults)
    {
        var allExercisesVariationsQuery = CreateExerciseVariationsQuery(context, includeInstructions: false, includePrerequisites: false);

        // We don't want to check if the user has ignored all variations if they are not in are section.
        // Only including exercises from the warmup, main, cooldown and the current section we are querying for,
        // ... so that that the rehab/prehab/sports sections will filter against their own section and the main three sections. 
        allExercisesVariationsQuery = Filters.FilterSection(allExercisesVariationsQuery, Section.Warmup | Section.Cooldown | Section.Main | section);

        // We don't want to check if the user has ignored all variations if they are not in are sports focus.
        // IncludeNone is true so we check against exercises that our a part of the normal strength training (non-SportsFocus) regimen.
        allExercisesVariationsQuery = Filters.FilterSportsFocus(allExercisesVariationsQuery, SportsOptions.SportsFocus, includeNone: true);

        // Further filter down the exercises to those that match our query results.
        allExercisesVariationsQuery = Filters.FilterExercises(allExercisesVariationsQuery, queryResults.Select(qr => qr.Exercise.Id).ToList());

        return await allExercisesVariationsQuery.Select(a => new AllVariationsQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
    }

    /// <summary>
    /// Grab a list of prerequisite exercises that we can check user progressions for.
    /// NOTE: Prerequisites failing the user is new + use caution check will be skipped.
    /// NOTE: Prerequisites that can't be seen due to the equipment check will be skipped.
    /// </summary>
    private async Task<List<PrerequisitesQueryResults>> GetPrerequisites(CoreContext context, IList<InProgressQueryResults> queryResults)
    {
        if (UserOptions.IgnorePrerequisites) { return []; }

        // Grab a half-filtered list of exercises to check prerequisites against.
        // This filters down to only variations that the user owns equipment for.
        var checkPrerequisitesFromQuery = CreateFilteredExerciseVariationsQuery(context, includeInstructions: false, includePrerequisites: false, ignoreExclusions: true);

        // Only check if the user's account is older than 1 week old.
        // Since UserExercise records are created on the fly, possible a prerequisite in another section won't apply until the next day.
        // ... Happens mostly when building the user's very first newsletter--UserExercises from subsequent sections are yet to be made.
        if (UserOptions.CreatedDate < StaleBeforeDate)
        {
            // Making sure the prerequisite has the potential to be seen by the user (within a recent timeframe).
            // Checking this so we don't get stuck not seeing an exercise if the prerequisite can't ever be seen.
            checkPrerequisitesFromQuery = checkPrerequisitesFromQuery.Where(a => a.UserExercise.LastVisible >= StaleBeforeDate);
        }

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
        return await checkPrerequisitesFromQuery.Select(a => new PrerequisitesQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
    }

    /// <summary>
    /// Check if the exercise prerequisite conditions are met.
    /// </summary>
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
                    // And the prerequisite exercise has been seen at least twice in the past.
                    // ... We don't want to show Handstand Pushups before showing the user Pushups.
                    && prerequisiteToCheck.UserExerciseLastSeen > prerequisiteToCheck.UserExerciseFirstSeen
                    // And all of the prerequisite's proficiency variations have been seen in the past.
                    // ... We don't want to show Handstand Pushups before showing the user Full Pushups.
                    && prerequisiteToCheck.UserVariationLastSeen > prerequisiteToCheck.UserVariationFirstSeen)
                {
                    continue;
                }

                // User is past the required proficiency level.
                if (prerequisiteToCheck.UserExerciseProgression > prerequisiteProficiency
                    // And the prerequisite exercise has been seen at least twice in the past.
                    // ... We don't want to show Handstand Pushups before showing the user Pushups.
                    && prerequisiteToCheck.UserExerciseLastSeen > prerequisiteToCheck.UserExerciseFirstSeen)
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
        if (UserOptions.NoUser) { return; }

        // This needs to be done before prerequisites are checked so that intermediary prerequisites are included in prerequisites.
        // Check this first so that the LastVisible date is not updated immediately after the UserExercise record is created.
        var userExercisesUpdated = new HashSet<UserExercise>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserExercise != null))
        {
            queryResult.UserExercise!.LastVisible = DateHelpers.Today;
            if (userExercisesUpdated.Add(queryResult.UserExercise))
            {
                context.UserExercises.Attach(queryResult.UserExercise);
                context.Entry(queryResult.UserExercise).Property(x => x.LastVisible).IsModified = true;
            }
        }

        var userExercisesCreated = new HashSet<UserExercise>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserExercise == null))
        {
            queryResult.UserExercise = new UserExercise()
            {
                UserId = UserOptions.Id,
                ExerciseId = queryResult.Exercise.Id,
                // If this is for rehab, start at the min progression level. Otherwise, start at the default progression level.
                Progression = Section.Rehab.HasFlag(section) ? UserConsts.UserProgressionMin : UserConsts.UserProgressionDefault
            };

            if (userExercisesCreated.Add(queryResult.UserExercise))
            {
                context.UserExercises.Add(queryResult.UserExercise);
            }
        }

        var userVariationsCreated = new HashSet<UserVariation>();
        foreach (var queryResult in queryResults.Where(qr => qr.UserVariation == null))
        {
            queryResult.UserVariation = new UserVariation()
            {
                VariationId = queryResult.Variation.Id,
                UserId = UserOptions.Id,
                Section = section
            };

            if (userVariationsCreated.Add(queryResult.UserVariation))
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

    /// <summary>
    /// Calculates what muscle groups haven't yet been worked by the <paramref name="finalResults"/>.
    /// </summary>
    private List<MusculoskeletalSystem> GetUnworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        return MuscleGroupOptions.MuscleTargetsRDA.Where(kv =>
        {
            // We are targeting this muscle group.
            var workedCount = finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have not overworked this muscle group and this muscle group is a part of our worked set.
            return workedCount < kv.Value && MuscleGroupOptions.AllMuscleGroups.HasFlag(kv.Key);
        }).Select(kv => kv.Key).ToList();
    }

    /// <summary>
    /// Calculates what muscle groups have been overworked by the <paramref name="finalResults"/>.
    /// </summary>
    private List<MusculoskeletalSystem> GetOverworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        // Not checking if this muscle group is a part of our worked set.
        // We don't want to overwork any muscle regardless if we are targeting it.
        return MuscleGroupOptions.MuscleTargetsTUL.Where(kv =>
        {
            var workedCount = finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have overworked this muscle group.
            return workedCount > kv.Value;
        }).Select(kv => kv.Key).ToList();
    }
}
