using Core.Models.Exercise;
using Core.Models.User;
using Data.Data.Query.Options;
using Data.Entities.User;

namespace Data.Data.Query;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class QueryBuilder
{
    private readonly CoreContext Context;

    private User? User;

    /// <summary>
    ///     Ignores global EF Core query filters to include soft-deleted entities.
    /// </summary>
    private readonly bool IgnoreGlobalQueryFilters = false;

    private ProficiencyOptions? ProficiencyOptions;
    private MovementPatternOptions? MovementPatternOptions;
    private MuscleGroupOptions? MuscleGroupOptions;
    private WeightOptions? WeightOptions;
    private SelectionOptions? SelectionOptions;
    private ExclusionOptions? ExclusionOptions;
    private ExerciseOptions? ExerciseOptions;
    private ExerciseTypeOptions? ExerciseTypeOptions;
    private ExerciseFocusOptions? ExerciseFocusOptions;
    private OrderByOptions? OrderByOptions;
    private SportsOptions? SportsOptions;
    private JointsOptions? JointsOptions;
    private EquipmentOptions? EquipmentOptions;
    private MuscleContractionsOptions? MuscleContractionsOptions;
    private MuscleMovementOptions? MuscleMovementOptions;

    /// <summary>
    /// Looks for similar buckets of exercise variations
    /// </summary>
    public QueryBuilder(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }

    /// <summary>
    /// Filter exercises down to the specified type.
    /// </summary>
    public QueryBuilder WithExerciseFocus(ExerciseFocus value, Action<ExerciseFocusOptions>? builder = null)
    {
        var options = ExerciseFocusOptions ?? new ExerciseFocusOptions(value);
        builder?.Invoke(options);
        ExerciseFocusOptions = options;
        return this;
    }

    /// <summary>
    /// Filter exercises down to the specified type.
    /// </summary>
    public QueryBuilder WithExerciseType(ExerciseType value, Action<ExerciseTypeOptions>? builder = null)
    {
        var options = ExerciseTypeOptions ?? new ExerciseTypeOptions(value);
        builder?.Invoke(options);
        ExerciseTypeOptions = options;
        return this;
    }

    /// <summary>
    /// Choose weighted variations of exercises before unweighted variations.
    /// </summary>
    public QueryBuilder WithOnlyWeights(bool? onlyWeights, Action<WeightOptions>? builder = null)
    {
        var options = WeightOptions ?? new WeightOptions(onlyWeights);
        builder?.Invoke(options);
        WeightOptions = options;
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public QueryBuilder WithProficency(Action<ProficiencyOptions>? builder = null)
    {
        var options = ProficiencyOptions ?? new ProficiencyOptions();
        builder?.Invoke(options);
        ProficiencyOptions = options;
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public QueryBuilder WithSelectionOptions(Action<SelectionOptions>? builder = null)
    {
        var options = SelectionOptions ?? new SelectionOptions();
        builder?.Invoke(options);
        SelectionOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle contractions.
    /// </summary>
    public QueryBuilder WithMuscleContractions(MuscleContractions muscleContractions, Action<MuscleContractionsOptions>? builder = null)
    {
        var options = MuscleContractionsOptions ?? new MuscleContractionsOptions(muscleContractions);
        builder?.Invoke(options);
        MuscleContractionsOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMuscleMovement(MuscleMovement muscleMovement, Action<MuscleMovementOptions>? builder = null)
    {
        var options = MuscleMovementOptions ?? new MuscleMovementOptions(muscleMovement);
        builder?.Invoke(options);
        MuscleMovementOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        var options = MovementPatternOptions ?? new MovementPatternOptions(movementPatterns);
        builder?.Invoke(options);
        MovementPatternOptions = options;
        return this;
    }

    /// <summary>
    /// Show exercises that work these unique muscle groups.
    /// </summary>
    public QueryBuilder WithMuscleGroups(MuscleGroups muscleGroups, Action<MuscleGroupOptions>? builder = null)
    {
        var options = MuscleGroupOptions ?? new MuscleGroupOptions(muscleGroups);
        builder?.Invoke(options);
        MuscleGroupOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to the user's progressions.
    /// </summary>
    public QueryBuilder WithUser(User? user, bool ignoreProgressions = false, bool ignorePrerequisites = false)
    {
        User = user;
        return WithSelectionOptions(options =>
        {
            options.IgnoreProgressions = ignoreProgressions;
            options.IgnorePrerequisites = ignorePrerequisites;
            options.UniqueExercises = true;
        });
    }

    /// <summary>
    /// Filter variations down to have this equipment.
    /// </summary>
    public QueryBuilder WithEquipment(IEnumerable<int> equipmentIds, Action<EquipmentOptions>? builder = null)
    {
        var options = EquipmentOptions ?? new EquipmentOptions(equipmentIds);
        builder?.Invoke(options);
        EquipmentOptions = options;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExcludeExercises(Action<ExclusionOptions>? builder = null)
    {
        var options = ExclusionOptions ?? new ExclusionOptions();
        builder?.Invoke(options);
        ExclusionOptions = options;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExercises(Action<ExerciseOptions>? builder = null)
    {
        var options = ExerciseOptions ?? new ExerciseOptions();
        builder?.Invoke(options);
        ExerciseOptions = options;
        return this;
    }

    /// <summary>
    /// Order the final results.
    /// </summary>
    public QueryBuilder WithOrderBy(OrderBy orderBy, Action<OrderByOptions>? builder = null)
    {
        var options = OrderByOptions ?? new OrderByOptions(orderBy);
        builder?.Invoke(options);
        OrderByOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this sport.
    /// </summary>
    public QueryBuilder WithSportsFocus(SportsFocus sportsFocus, Action<SportsOptions>? builder = null)
    {
        var options = SportsOptions ?? new SportsOptions(sportsFocus);
        builder?.Invoke(options);
        SportsOptions = options;
        return this;
    }

    public QueryBuilder WithJoints(Joints joints, Action<JointsOptions>? builder = null)
    {
        var options = JointsOptions ?? new JointsOptions(joints);
        builder?.Invoke(options);
        JointsOptions = options;
        return this;
    }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public QueryRunner Build()
    {
        return new QueryRunner(Context, ignoreGlobalQueryFilters: IgnoreGlobalQueryFilters)
        {
            User = User,
            MuscleGroup = MuscleGroupOptions ?? new MuscleGroupOptions(),
            WeightOptions = WeightOptions ?? new WeightOptions(),
            MovementPattern = MovementPatternOptions ?? new MovementPatternOptions(),
            Proficiency = ProficiencyOptions ?? new ProficiencyOptions(),
            ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
            ExerciseOptions = ExerciseOptions ?? new ExerciseOptions(),
            ExerciseTypeOptions = ExerciseTypeOptions ?? new ExerciseTypeOptions(),
            OrderByOptions = OrderByOptions ?? new OrderByOptions(),
            SelectionOptions = SelectionOptions ?? new SelectionOptions(),
            SportsOptions = SportsOptions ?? new SportsOptions(),
            JointsOptions = JointsOptions ?? new JointsOptions(),
            MuscleContractionsOptions = MuscleContractionsOptions ?? new MuscleContractionsOptions(),
            MuscleMovementOptions = MuscleMovementOptions ?? new MuscleMovementOptions(),
            EquipmentOptions = EquipmentOptions ?? new EquipmentOptions(),
            ExerciseFocusOptions = ExerciseFocusOptions ?? new ExerciseFocusOptions(),
        };
    }
}