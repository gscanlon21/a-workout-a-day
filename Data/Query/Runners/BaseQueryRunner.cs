using Core.Dtos.Exercise;
using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Entities.Exercises;
using Data.Entities.Users;
using Data.Query.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;

namespace Data.Query.Runners;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
public abstract class BaseQueryRunner
{
    protected readonly Section _section;

    public BaseQueryRunner(Section section)
    {
        _section = section;
    }

    [DebuggerDisplay("{Exercise}: {Variation}")]
    protected class ExerciseVariation
        : IExerciseVariationCombo
    {
        /// <summary>EF Core can't optimize constructors.</summary>
        public ExerciseVariation() { /* no-op */}

        public required Exercise Exercise { get; init; }
        public required Variation Variation { get; init; }
        public required UserExercise UserExercise { get; init; }
        public required UserVariation UserVariation { get; init; }
        public required IList<ExercisePrerequisiteDto> Prerequisites { get; init; }
        public required IList<ExercisePrerequisiteDto> Postrequisites { get; init; }
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
    public class ExerciseVariationsQueryResults
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
    public class InProgressQueryResults(ExerciseVariationsQueryResults queryResult)
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

    public required SkillsOptions SkillsOptions { private get; init; }
    public required EquipmentOptions EquipmentOptions { private get; init; }
    public required ExerciseFocusOptions ExerciseFocusOptions { private get; init; }
    public required MuscleMovementOptions MuscleMovementOptions { private get; init; }

    public required SportsOptions SportsOptions { protected get; init; }
    public required ExerciseOptions ExerciseOptions { protected get; init; }
    public required SelectionOptions SelectionOptions { protected get; init; }
    public required ExclusionOptions ExclusionOptions { protected get; init; }
    public required MuscleGroupOptions MuscleGroupOptions { protected get; init; }
    public required MovementPatternOptions MovementPatternOptions { protected get; init; }

    protected DateOnly StaleBeforeDate { get; } = DateHelpers.Today.AddDays(-ExerciseConsts.StaleAfterDays);

    protected IQueryable<Exercise> CreateExercisesQuery(CoreContext context)
    {
        return context.Exercises.IgnoreQueryFilters().TagWith(nameof(CreateExercisesQuery))
            .Where(ev => ev.DisabledReason == null);
    }

    protected IQueryable<Variation> CreateVariationsQuery(CoreContext context, bool includeInstructions)
    {
        var query = context.Variations.IgnoreQueryFilters().TagWith(nameof(CreateVariationsQuery))
            .Where(ev => ev.DisabledReason == null);

        if (includeInstructions)
        {
            // Include root instructions and then include those root instruction's child and grandchild instructions.
            return query.Include(v => v.Instructions.Where(c => c.DisabledReason == null).Where(i => i.Parent == null))
                // Not filtering out the grandchild's DisabledReason for performance. The parent is likely enabled.
                .ThenInclude(i => i.Children.Where(c => c.DisabledReason == null)).ThenInclude(i => i.Children);
        }

        return query;
    }

    protected IQueryable<ExerciseVariation> CreateExerciseVariationsQuery(CoreContext context, bool includeInstructions, bool includePrerequisites)
    {
        return Map(CreateExercisesQuery(context), includePrerequisites: includePrerequisites)
            .Join(Map(CreateVariationsQuery(context, includeInstructions: includeInstructions)),
                o => o.Exercise.Id, i => i.Variation.ExerciseId, (o, i) => new ExerciseVariation()
                {
                    Exercise = o.Exercise,
                    UserExercise = o.UserExercise,
                    Variation = i.Variation,
                    UserVariation = i.UserVariation,
                    Prerequisites = o.Prerequisites,
                    Postrequisites = o.Postrequisites,
                });
    }

    protected abstract IQueryable<ExercisesQueryResults> Map(IQueryable<Exercise> exercises, bool includePrerequisites);
    protected abstract IQueryable<VariationsQueryResults> Map(IQueryable<Variation> variations);
    protected abstract IQueryable<ExerciseVariationsQueryResults> Map(IQueryable<ExerciseVariation> exerciseVariations);

    protected virtual IQueryable<ExerciseVariationsQueryResults> Filter(IQueryable<ExerciseVariationsQueryResults> exerciseVariations, bool ignoreIgnored = false, bool ignoreExclusions = false)
    {
        // Don't apply these to prerequisites.
        if (!ignoreExclusions)
        {
            // Don't grab exercises that we want to ignore.
            if (ExclusionOptions.ExerciseIds.Any())
            {
                exerciseVariations = exerciseVariations.Where(vm => !ExclusionOptions.ExerciseIds.Contains(vm.Exercise.Id));
            }

            // Don't grab variations that we want to ignore.
            if (ExclusionOptions.VariationIds.Any())
            {
                exerciseVariations = exerciseVariations.Where(vm => !ExclusionOptions.VariationIds.Contains(vm.Variation.Id));
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.VocalSkills != VocalSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                exerciseVariations = exerciseVariations.Where(vm => vm.Exercise.VocalSkills == 0 || (ExclusionOptions.VocalSkills & vm.Exercise.VocalSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.VisualSkills != VisualSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                exerciseVariations = exerciseVariations.Where(vm => vm.Exercise.VisualSkills == 0 || (ExclusionOptions.VisualSkills & vm.Exercise.VisualSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.CervicalSkills != CervicalSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                exerciseVariations = exerciseVariations.Where(vm => vm.Exercise.CervicalSkills == 0 || (ExclusionOptions.CervicalSkills & vm.Exercise.CervicalSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.ThoracicSkills != ThoracicSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                exerciseVariations = exerciseVariations.Where(vm => vm.Exercise.ThoracicSkills == 0 || (ExclusionOptions.ThoracicSkills & vm.Exercise.ThoracicSkills) == 0);
            }

            // Don't grab skills that we want to ignore.
            if (ExclusionOptions.LumbarSkills != LumbarSkills.None)
            {
                // Include exercises where the skill type is not the same as one we want to exclude, or the skills used by the exercise are all different.
                exerciseVariations = exerciseVariations.Where(vm => vm.Exercise.LumbarSkills == 0 || (ExclusionOptions.LumbarSkills & vm.Exercise.LumbarSkills) == 0);
            }

            // Filter out padded refresh variations.
            if (SelectionOptions.OnlyRefreshed)
            {
                // Include lagged refresh variations (RefreshAfter != null), so they always show up while pending refresh.
                exerciseVariations = exerciseVariations.Where(vm => vm.UserVariation.LastSeen == null || vm.UserVariation.LastSeen <= DateHelpers.Today);
            }
        }

        return exerciseVariations;
    }

    /// <summary>
    /// Queries the db for the data.
    /// </summary>
    protected async Task<List<InProgressQueryResults>> QueryPartial(CoreContext context)
    {
        var filteredQuery = Filter(Map(CreateExerciseVariationsQuery(context,
            includePrerequisites: SelectionOptions.IncludePrerequisites,
            includeInstructions: SelectionOptions.IncludeInstructions)));

        filteredQuery = QueryFilters.FilterSection(filteredQuery, _section);
        filteredQuery = QueryFilters.FilterSkills(filteredQuery, SkillsOptions);
        filteredQuery = QueryFilters.FilterEquipment(filteredQuery, EquipmentOptions.Equipment);
        filteredQuery = QueryFilters.FilterSportsFocus(filteredQuery, SportsOptions.SportsFocus);
        filteredQuery = QueryFilters.FilterExercises(filteredQuery, ExerciseOptions.ExerciseIds);
        filteredQuery = QueryFilters.FilterVariations(filteredQuery, ExerciseOptions.VariationIds);
        filteredQuery = QueryFilters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExerciseFocus);
        filteredQuery = QueryFilters.FilterMuscleMovement(filteredQuery, MuscleMovementOptions.MuscleMovement);
        filteredQuery = QueryFilters.FilterMovementPattern(filteredQuery, MovementPatternOptions.MovementPatterns);
        filteredQuery = QueryFilters.FilterExerciseFocus(filteredQuery, ExerciseFocusOptions.ExcludeExerciseFocus, exclude: true);
        filteredQuery = QueryFilters.FilterMuscleGroup(filteredQuery, MuscleGroupOptions.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (c, n) => c | n), include: true, MuscleGroupOptions.MuscleTarget);
        //filteredQuery = QueryFilters.FilterMuscleGroup(filteredQuery, UserOptions.ExcludeRecoveryMuscle, include: false, UserOptions.ExcludeRecoveryMuscleTarget);

        // When you perform comparisons with nullable types, if the value of one of the nullable types
        // ... is null and the other is not, all comparisons evaluate to false except for != (not equal).
        return await filteredQuery.Select(a => new InProgressQueryResults(a)).AsNoTracking().TagWithCallSite().ToListAsync();
    }

    public abstract Task<IList<QueryResults>> Query(IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue);
}
