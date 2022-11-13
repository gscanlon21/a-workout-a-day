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

    /// <summary>
    ///     If true and the User's exercise proficiency is above the exercise's proficiency:
    ///     ... Will choose exercise that fall at or under the exercise's proficiency level.
    ///     Otherwise, will choose variations that fall within the User's exiercise progression range. 
    /// </summary>
    private bool DoCapAtProficiency = false;

    private ExerciseType? ExerciseType;
    private MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    private bool? IncludeBonus;
    private MuscleContractions? MuscleContractions;
    private MuscleMovement? MuscleMovement;
    private IntensityLevel? IntensityLevel;
    private OrderByEnum OrderBy = OrderByEnum.None;
    private SportsFocus? SportsFocus;
    private int SkipCount = 0;
    private bool? Unilateral = null;
    private IEnumerable<int>? EquipmentIds;
    private IEnumerable<int>? ExerciseExclusions;

    private MovementPatternOptions MovementPattern = new MovementPatternOptions();
    private MuscleGroupOptions MuscleGroup = new MuscleGroupOptions();
    private WeightOptions WeightOptions = new WeightOptions();

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

    /// <summary>
    /// Don't choose variations where the exercise min progression is greater than the exercise proficiency level.
    /// For things like warmups--rather have regular pushups over one-hand pushups.
    /// </summary>
    public ExerciseQueryBuilder CapAtProficiency(bool doCap)
    {
        DoCapAtProficiency = doCap;
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
    /// Filter exercises down to this intensity level
    /// </summary>
    public ExerciseQueryBuilder WithIntensityLevel(IntensityLevel intensityLevel)
    {
        IntensityLevel = intensityLevel;
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
            DoCapAtProficiency = DoCapAtProficiency,
            MuscleContractions = MuscleContractions,
            MuscleMovement = MuscleMovement,
            MusclesAlreadyWorked = MusclesAlreadyWorked,
            EquipmentIds = EquipmentIds,
            ExerciseExclusions = ExerciseExclusions,
            ExerciseType = ExerciseType,
            SkipCount = SkipCount,
            IncludeBonus = IncludeBonus,
            OrderBy = OrderBy,
            Unilateral = Unilateral,
            RecoveryMuscle = RecoveryMuscle,
            IntensityLevel = IntensityLevel,
            SportsFocus = SportsFocus,
        };
    }
}