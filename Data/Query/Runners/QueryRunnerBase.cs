using Core.Dtos.Exercise;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Code.Extensions;
using Data.Entities.Exercise;
using Data.Entities.Users;
using Data.Query.Options;
using Data.Query.Options.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Data.Query.Runners;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
public abstract class QueryRunnerBase
{
    protected readonly Section section;

    public QueryRunnerBase(Section sec)
    {
        section = sec;
    }

    [DebuggerDisplay("{Exercise}")]
    protected class ExercisesQueryResults
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public ExercisesQueryResults() { /* no-op */}

        public required Exercise Exercise { get; init; }
        public required UserExercise UserExercise { get; init; }
        public IList<ExercisePrerequisiteDto> Prerequisites { get; init; } = [];
        public IList<ExercisePrerequisiteDto> Postrequisites { get; init; } = [];
    }

    [DebuggerDisplay("{Variation}")]
    protected class VariationsQueryResults
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public VariationsQueryResults() { /* no-op */}

        public required Variation Variation { get; init; }
        public required UserVariation UserVariation { get; init; }
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    protected class ExerciseVariationsQueryResults
        : IExerciseVariationCombo
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public ExerciseVariationsQueryResults() { /* no-op */}

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
    protected class InProgressQueryResults(ExerciseVariationsQueryResults queryResult)
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

    [DebuggerDisplay("{VariationId}: {VariationName}")]
    protected class AllVariationsQueryResults
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public AllVariationsQueryResults() { /* no-op */}

        public required string Name { get; init; }
        public required int ExerciseId { get; init; }
        public required int VariationId { get; init; }
        public required bool IsIgnored { get; init; }
        public required bool UseCaution { get; init; }
        public required bool UserOwnsEquipment { get; init; }
        public required bool IsMinProgressionInRange { get; init; }
        public required bool IsMaxProgressionInRange { get; init; }
        public bool IsProgressionInRange => IsMinProgressionInRange && IsMaxProgressionInRange;
        public required Progression VariationProgression { get; init; }
        public required DateOnly? LastVisible { get; init; }
    }

    [DebuggerDisplay("{GetDebuggerDisplay(),nq}")]
    protected class PrerequisitesQueryResults
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public PrerequisitesQueryResults() { /* no-op */}

        public required int ExerciseId { get; init; }
        public required DateOnly? UserExerciseLastSeen { get; init; }
        public required DateOnly? UserExerciseFirstSeen { get; init; }
        public required DateOnly? UserVariationLastSeen { get; init; }
        public required DateOnly? UserVariationFirstSeen { get; init; }

        public required Progression VariationProgression { get; init; }
        public required int UserExerciseProgression { get; init; }

        private string GetDebuggerDisplay() => $"{VariationProgression.MinOrDefault} - {UserExerciseProgression} - {VariationProgression.MaxOrDefault}";
    }

    public required UserOptions UserOptions { get; init; }
    public required UserIgnoreOptions UserIgnoreOptions { get; init; }

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

    protected DateOnly StaleBeforeDate { get; } = DateHelpers.Today.AddDays(-ExerciseConsts.StaleAfterDays);

    protected IQueryable<ExercisesQueryResults> CreateExercisesQuery(CoreContext context, bool includePrerequisites)
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

    protected IQueryable<VariationsQueryResults> CreateVariationsQuery(CoreContext context, bool includeInstructions)
    {
        var query = context.Variations.IgnoreQueryFilters().TagWith(nameof(CreateVariationsQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includeInstructions)
        {
            // Include root instructions and then include those root instruction's child and grandchild instructions.
            query = query.Include(v => v.Instructions.Where(c => c.DisabledReason == null).Where(i => i.Parent == null))
                // Not filtering out the grandchild's DisabledReason for performance. The parent is likely enabled.
                .ThenInclude(i => i.Children.Where(c => c.DisabledReason == null)).ThenInclude(i => i.Children);
        }

        return query.Select(v => new VariationsQueryResults()
        {
            Variation = v,
            UserVariation = v.UserVariations.First(uv => !UserOptions.NoUser && uv.UserId == UserOptions.Id && uv.Section == section)
        });
    }

    protected IQueryable<ExerciseVariationsQueryResults> CreateExerciseVariationsQuery(CoreContext context, bool includeInstructions, bool includePrerequisites)
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

    protected IQueryable<ExerciseVariationsQueryResults> CreateFilteredExerciseVariationsQuery(CoreContext context, bool includeInstructions, bool includePrerequisites, bool ignoreExclusions = false)
    {
        var filteredQuery = CreateExerciseVariationsQuery(context,
                includeInstructions: includeInstructions,
                includePrerequisites: includePrerequisites)
            .TagWith(nameof(CreateFilteredExerciseVariationsQuery));

        // Not in a workout context, ignore user filtering.
        if (!UserOptions.NoUser && section != Section.None)
        {
            // Filter down to variations the user owns equipment for.
            filteredQuery = filteredQuery.Where(vm => vm.UserOwnsEquipment);

            if (UserIgnoreOptions.UserExercises)
            {
                // Don't grab exercises that the user wants to ignore.
                filteredQuery = filteredQuery.Where(vm => vm.UserExercise.Ignore != true);
            }

            if (UserIgnoreOptions.UserVariations)
            {
                // Don't grab variations that the user wants to ignore.
                filteredQuery = filteredQuery.Where(vm => vm.UserVariation.Ignore != true);
            }
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
    public abstract Task<IList<QueryResults>> Query(IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue);
}
