using Web.Data.Query.Options;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Data.Query;

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

    private ProficiencyOptions? Proficiency;
    private MovementPatternOptions? MovementPattern;
    private MuscleGroupOptions? MuscleGroup;
    private WeightOptions? WeightOptions;
    private ExclusionOptions? ExclusionOptions;
    private ExerciseOptions? ExerciseOptions;

    // TODO: Move these into options classes
    private ExerciseFocus? ExerciseFocus;
    private ExerciseType? ExerciseType;
    private MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    private MuscleContractions? MuscleContractions;
    private MuscleMovement? MuscleMovement;
    private OrderBy OrderBy = OrderBy.None;
    private SportsFocus? SportsFocus;
    private Joints? Joints;
    private int SkipCount = 0;
    private bool UniqueExercises = false;
    private bool? Unilateral = null;
    private bool? AntiGravity = null;
    private IEnumerable<int>? EquipmentIds;

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
    public QueryBuilder WithExerciseFocus(ExerciseFocus value)
    {
        ExerciseFocus = value;
        return this;
    }

    /// <summary>
    /// Filter exercises down to the specified type.
    /// </summary>
    public QueryBuilder WithExerciseType(ExerciseType value)
    {
        ExerciseType = value;
        return this;
    }

    /// <summary>
    /// Choose weighted variations of exercises before unweighted variations.
    /// </summary>
    public QueryBuilder WithOnlyWeights(bool? onlyWeights, Action<WeightOptions>? builder = null)
    {
        var options = new WeightOptions(onlyWeights);
        builder?.Invoke(options);
        WeightOptions = options;
        return this;
    }

    public QueryBuilder WithAntiGravity(bool? antiGravity)
    {
        AntiGravity = antiGravity;
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public QueryBuilder WithProficency(Action<ProficiencyOptions>? builder = null)
    {
        var options = new ProficiencyOptions();
        builder?.Invoke(options);
        Proficiency = options;
        return this;
    }

    /// <summary>
    /// Filter exercises down to unilateral variations.
    /// </summary>
    public QueryBuilder IsUnilateral(bool? isUnilateral)
    {
        Unilateral = isUnilateral;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle contractions.
    /// </summary>
    public QueryBuilder WithMuscleContractions(MuscleContractions muscleContractions)
    {
        MuscleContractions = muscleContractions;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMuscleMovement(MuscleMovement muscleMovement)
    {
        MuscleMovement = muscleMovement;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        var options = new MovementPatternOptions(movementPatterns);
        builder?.Invoke(options);
        MovementPattern = options;
        return this;
    }

    /// <summary>
    /// Show exercises that work these unique muscle groups.
    /// </summary>
    public QueryBuilder WithMuscleGroups(MuscleGroups muscleGroups, Action<MuscleGroupOptions>? builder = null)
    {
        var options = new MuscleGroupOptions(muscleGroups);
        builder?.Invoke(options);
        MuscleGroup = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to the user's progressions.
    /// </summary>
    public QueryBuilder WithUser(User? user)
    {
        UniqueExercises = true;
        User = user;
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment.
    /// </summary>
    public QueryBuilder WithEquipment(IEnumerable<int> equipmentIds)
    {
        EquipmentIds = equipmentIds;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExcludeExercises(Action<ExclusionOptions>? builder = null)
    {
        var options = new ExclusionOptions();
        builder?.Invoke(options);
        ExclusionOptions = options;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExercises(Action<ExerciseOptions>? builder = null)
    {
        var options = new ExerciseOptions();
        builder?.Invoke(options);
        ExerciseOptions = options;
        return this;
    }

    /// <summary>
    /// Already worked muscle groups.
    /// </summary>
    public QueryBuilder AddAlreadyWorkedMuscles(MuscleGroups muscleGroups)
    {
        MusclesAlreadyWorked |= muscleGroups;
        return this;
    }

    /// <summary>
    /// Order the final results.
    /// </summary>
    public QueryBuilder WithOrderBy(OrderBy orderBy, int skip = 0)
    {
        SkipCount = skip;
        OrderBy = orderBy;
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this sport.
    /// </summary>
    public QueryBuilder WithSportsFocus(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
        return this;
    }

    public QueryBuilder WithJoints(Joints sportsFocus)
    {
        Joints = sportsFocus;
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
            MuscleGroup = MuscleGroup ?? new MuscleGroupOptions(),
            WeightOptions = WeightOptions ?? new WeightOptions(),
            MovementPattern = MovementPattern ?? new MovementPatternOptions(),
            Proficiency = Proficiency ?? new ProficiencyOptions(),
            ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
            ExerciseOptions = ExerciseOptions ?? new ExerciseOptions(),
            MuscleContractions = MuscleContractions,
            MuscleMovement = MuscleMovement,
            MusclesAlreadyWorked = MusclesAlreadyWorked,
            EquipmentIds = EquipmentIds,
            ExerciseFocus = ExerciseFocus,
            ExerciseType = ExerciseType,
            SkipCount = SkipCount,
            AntiGravity = AntiGravity,
            OrderBy = OrderBy,
            UniqueExercises = UniqueExercises,
            Unilateral = Unilateral,
            SportsFocus = SportsFocus,
            Joints = Joints,
        };
    }
}