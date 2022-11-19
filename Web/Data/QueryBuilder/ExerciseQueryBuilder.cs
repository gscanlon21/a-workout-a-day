using Web.Entities.Exercise;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Data.QueryBuilder;

public class ExerciseQueryBuilder
{
    public enum OrderByEnum
    {
        None,

        /// <summary>
        ///     Orders variations by their min progression ASC and then max progression ASC
        /// </summary>
        Progression,

        /// <summary>
        ///     Orders exercises by how many muscles they target of MuscleGroups.
        /// </summary>
        MuscleTarget,

        /// <summary>
        ///     Chooses exercises based on how many unique muscles the variation targets that have not already been worked.
        /// </summary>
        UniqueMuscles,

        /// <summary>
        ///     Orders variations by their exercise name ASC and then their variation name ASC.
        /// </summary>
        Name
    }

    private readonly CoreContext Context;

    private User? User;

    /// <summary>
    ///     Ignores global EF Core query filters to include soft-deleted entities.
    /// </summary>
    private readonly bool IgnoreGlobalQueryFilters = false;

    /// <summary>
    ///     If null, includes the recovery tracks in the queried exercises.
    ///     If MuscleGroups.None, excludes the recovery tracks from the queried exercises.
    ///     If > MuscleGroups.None, only queries exercises from that recovery muscle track.
    /// </summary>
    private MuscleGroups? RecoveryMuscle;

    private ExerciseType? ExerciseType;
    private MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    private bool? IncludeBonus;
    private MuscleContractions? MuscleContractions;
    private MuscleMovement? MuscleMovement;
    private OrderByEnum OrderBy = OrderByEnum.None;
    private SportsFocus? SportsFocus;
    private int SkipCount = 0;
    private bool? Unilateral = null;
    private bool? AntiGravity = null;
    private IEnumerable<int>? EquipmentIds;
    private IEnumerable<int>? ExerciseExclusions;

    private ProficiencyOptions? Proficiency;
    private MovementPatternOptions? MovementPattern;
    private MuscleGroupOptions? MuscleGroup;
    private WeightOptions? WeightOptions;

    public ExerciseQueryBuilder(CoreContext context, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
    }

    /// <summary>
    /// Filter exercises down to the specified type
    /// </summary>
    public ExerciseQueryBuilder WithExerciseType(ExerciseType exerciseType)
    {
        ExerciseType = exerciseType;
        return this;
    }

    /// <summary>
    ///     Choose weighted variations of exercises before unweighted variations.
    /// </summary>
    public ExerciseQueryBuilder WithPrefersWeights(bool? prefersWeights, Action<WeightOptions>? builder = null)
    {
        var options = new WeightOptions(prefersWeights);
        builder?.Invoke(options);
        WeightOptions = options;
        return this;
    }

    public ExerciseQueryBuilder WithIncludeBonus(bool? includeBonus)
    {
        IncludeBonus = includeBonus;
        return this;
    }

    public ExerciseQueryBuilder WithAntiGravity(bool? antiGravity)
    {
        AntiGravity = antiGravity;
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public ExerciseQueryBuilder WithProficency(Action<ProficiencyOptions>? builder = null)
    {
        var options = new ProficiencyOptions();
        builder?.Invoke(options);
        Proficiency = options;
        return this;
    }

    /// <summary>
    /// Filter exercises down to unilateral variations
    /// </summary>
    public ExerciseQueryBuilder IsUnilateral(bool? isUnilateral)
    {
        Unilateral = isUnilateral;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle contractions
    /// </summary>
    public ExerciseQueryBuilder WithMuscleContractions(MuscleContractions muscleContractions)
    {
        MuscleContractions = muscleContractions;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement
    /// </summary>
    public ExerciseQueryBuilder WithMuscleMovement(MuscleMovement muscleMovement)
    {
        MuscleMovement = muscleMovement;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement
    /// </summary>
    public ExerciseQueryBuilder WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        var options = new MovementPatternOptions(movementPatterns);
        builder?.Invoke(options);
        MovementPattern = options;
        return this;
    }

    public ExerciseQueryBuilder WithMuscleGroups(MuscleGroups muscleGroups, Action<MuscleGroupOptions>? builder = null)
    {
        var options = new MuscleGroupOptions(muscleGroups);
        builder?.Invoke(options);
        MuscleGroup = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to the user's progressions
    /// </summary>
    public ExerciseQueryBuilder WithUser(User? user)
    {
        User = user;
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment
    /// </summary>
    public ExerciseQueryBuilder WithEquipment(IEnumerable<int> equipmentIds)
    {
        EquipmentIds = equipmentIds;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids
    /// </summary>
    public ExerciseQueryBuilder WithExcludeExercises(IEnumerable<int> exerciseIds)
    {
        ExerciseExclusions = exerciseIds;
        return this;
    }

    /// <summary>
    /// Already worked muscle groups
    /// </summary>
    public ExerciseQueryBuilder WithAlreadyWorkedMuscles(MuscleGroups muscleGroups)
    {
        MusclesAlreadyWorked = muscleGroups;
        return this;
    }

    /// <summary>
    /// Order the final results
    /// </summary>
    public ExerciseQueryBuilder WithOrderBy(OrderByEnum orderBy, int skip = 0)
    {
        SkipCount = skip;
        OrderBy = orderBy;
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this spoirt
    /// </summary>
    public ExerciseQueryBuilder WithSportsFocus(SportsFocus sportsFocus)
    {
        SportsFocus = sportsFocus;
        return this;
    }

    /// <summary>
    ///     Return only exercises that are a part of the recovery muscle's track.
    /// </summary>
    /// <param name="recoveryMuscle">
    ///     <inheritdoc cref="RecoveryMuscle"/>
    /// </param>
    public ExerciseQueryBuilder WithRecoveryMuscle(MuscleGroups recoveryMuscle)
    {
        RecoveryMuscle = recoveryMuscle;
        return this;
    }

    public ExerciseQueryer Build()
    {
        return new ExerciseQueryer(Context, IgnoreGlobalQueryFilters)
        {
            User = User,
            MuscleGroup = MuscleGroup ?? new MuscleGroupOptions(),
            WeightOptions = WeightOptions ?? new WeightOptions(),
            MovementPattern = MovementPattern ?? new MovementPatternOptions(),
            Proficiency = Proficiency ?? new ProficiencyOptions(),
            MuscleContractions = MuscleContractions,
            MuscleMovement = MuscleMovement,
            MusclesAlreadyWorked = MusclesAlreadyWorked,
            EquipmentIds = EquipmentIds,
            ExerciseExclusions = ExerciseExclusions,
            ExerciseType = ExerciseType,
            SkipCount = SkipCount,
            IncludeBonus = IncludeBonus,
            AntiGravity = AntiGravity,
            OrderBy = OrderBy,
            Unilateral = Unilateral,
            RecoveryMuscle = RecoveryMuscle,
            SportsFocus = SportsFocus,
        };
    }
}