﻿using Web.Data.Query.Options;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Data.Query;

public class QueryBuilder
{
    private readonly CoreContext Context;

    private User? User;

    /// <summary>
    ///     Ignores global EF Core query filters to include soft-deleted entities.
    /// </summary>
    private readonly bool IgnoreGlobalQueryFilters = false;

    /// <summary>
    ///     Refreshes the core set of exercises.
    /// </summary>
    public bool? Refresh { get; set; } = null;

    /// <summary>
    ///     If null, includes the recovery tracks in the queried exercises.
    ///     If MuscleGroups.None, excludes the recovery tracks from the queried exercises.
    ///     If > MuscleGroups.None, only queries exercises from that recovery muscle track.
    /// </summary>
    private MuscleGroups? RecoveryMuscle;

    private ProficiencyOptions? Proficiency;
    private MovementPatternOptions? MovementPattern;
    private MuscleGroupOptions? MuscleGroup;
    private WeightOptions? WeightOptions;
    private ExclusionOptions? ExclusionOptions;

    // TODO: Move these into options classes
    private ExerciseType? ExerciseType;
    private MuscleGroups MusclesAlreadyWorked = MuscleGroups.None;
    private MuscleContractions? MuscleContractions;
    private MuscleMovement? MuscleMovement;
    private OrderBy OrderBy = OrderBy.None;
    private SportsFocus? SportsFocus;
    private int SkipCount = 0;
    private bool? Unilateral = null;
    private bool? AntiGravity = null;
    private IEnumerable<int>? EquipmentIds;

    public QueryBuilder(CoreContext context, bool? refresh = null, bool ignoreGlobalQueryFilters = false)
    {
        Context = context;
        IgnoreGlobalQueryFilters = ignoreGlobalQueryFilters;
        Refresh = refresh;
    }

    /// <summary>
    /// Filter exercises down to the specified type
    /// </summary>
    public QueryBuilder WithExerciseType(ExerciseType exerciseType)
    {
        ExerciseType = exerciseType;
        return this;
    }

    /// <summary>
    ///     Choose weighted variations of exercises before unweighted variations.
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
    /// Filter exercises down to unilateral variations
    /// </summary>
    public QueryBuilder IsUnilateral(bool? isUnilateral)
    {
        Unilateral = isUnilateral;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle contractions
    /// </summary>
    public QueryBuilder WithMuscleContractions(MuscleContractions muscleContractions)
    {
        MuscleContractions = muscleContractions;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement
    /// </summary>
    public QueryBuilder WithMuscleMovement(MuscleMovement muscleMovement)
    {
        MuscleMovement = muscleMovement;
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement
    /// </summary>
    public QueryBuilder WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        var options = new MovementPatternOptions(movementPatterns);
        builder?.Invoke(options);
        MovementPattern = options;
        return this;
    }

    public QueryBuilder WithMuscleGroups(MuscleGroups muscleGroups, Action<MuscleGroupOptions>? builder = null)
    {
        var options = new MuscleGroupOptions(muscleGroups);
        builder?.Invoke(options);
        MuscleGroup = options;
        return this;
    }

    /// <summary>
    /// Filter variations down to the user's progressions
    /// </summary>
    public QueryBuilder WithUser(User? user)
    {
        User = user;
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment
    /// </summary>
    public QueryBuilder WithEquipment(IEnumerable<int> equipmentIds)
    {
        EquipmentIds = equipmentIds;
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids
    /// </summary>
    public QueryBuilder WithExcludeExercises(Action<ExclusionOptions>? builder = null)
    {
        var options = new ExclusionOptions();
        builder?.Invoke(options);
        ExclusionOptions = options;
        return this;
    }

    /// <summary>
    /// Already worked muscle groups
    /// </summary>
    public QueryBuilder WithAlreadyWorkedMuscles(MuscleGroups muscleGroups)
    {
        MusclesAlreadyWorked = muscleGroups;
        return this;
    }

    /// <summary>
    /// Order the final results
    /// </summary>
    public QueryBuilder WithOrderBy(OrderBy orderBy, int skip = 0)
    {
        SkipCount = skip;
        OrderBy = orderBy;
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this spoirt
    /// </summary>
    public QueryBuilder WithSportsFocus(SportsFocus sportsFocus)
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
    public QueryBuilder WithRecoveryMuscle(MuscleGroups recoveryMuscle)
    {
        RecoveryMuscle = recoveryMuscle;
        return this;
    }

    public QueryRunner Build()
    {
        return new QueryRunner(Context, refresh: Refresh, ignoreGlobalQueryFilters: IgnoreGlobalQueryFilters)
        {
            User = User,
            MuscleGroup = MuscleGroup ?? new MuscleGroupOptions(),
            WeightOptions = WeightOptions ?? new WeightOptions(),
            MovementPattern = MovementPattern ?? new MovementPatternOptions(),
            Proficiency = Proficiency ?? new ProficiencyOptions(),
            ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
            MuscleContractions = MuscleContractions,
            MuscleMovement = MuscleMovement,
            MusclesAlreadyWorked = MusclesAlreadyWorked,
            EquipmentIds = EquipmentIds,
            ExerciseType = ExerciseType,
            SkipCount = SkipCount,
            AntiGravity = AntiGravity,
            OrderBy = OrderBy,
            Unilateral = Unilateral,
            RecoveryMuscle = RecoveryMuscle,
            SportsFocus = SportsFocus,
        };
    }
}