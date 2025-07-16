using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.User;
using Data.Query.Options;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class QueryBuilder
{
    private readonly Section Section;

    private UserOptions? UserOptions;
    private MovementPatternOptions? MovementPatternOptions;
    private MuscleGroupOptions? MuscleGroupOptions;
    private SelectionOptions? SelectionOptions;
    private ExclusionOptions? ExclusionOptions;
    private ExerciseOptions? ExerciseOptions;
    private ExerciseFocusOptions? ExerciseFocusOptions;
    private SportsOptions? SportsOptions;
    private SkillsOptions? SkillsOptions;
    private EquipmentOptions? EquipmentOptions;
    private MuscleMovementOptions? MuscleMovementOptions;

    /// <summary>
    /// Looks for similar buckets of exercise variations.
    /// </summary>
    public QueryBuilder(Section section)
    {
        Section = section;
    }

    /// <summary>
    /// Filter exercises down to the specified type.
    /// </summary>
    /// <param name="value">Will filter down to any of the flags values.</param>
    public QueryBuilder WithExerciseFocus(IList<ExerciseFocus> value, Action<ExerciseFocusOptions>? builder = null)
    {
        var options = ExerciseFocusOptions ?? new ExerciseFocusOptions(value);
        builder?.Invoke(options);
        ExerciseFocusOptions = options;
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
    public QueryBuilder WithMuscleGroups(IMuscleGroupBuilderFinalNoContext builder, Action<MuscleGroupOptions>? optionsBuilder = null)
    {
        var options = builder.Build(Section);
        optionsBuilder?.Invoke(options);
        MuscleGroupOptions = options;
        return this;
    }

    /// <summary>
    /// Filter variations according to the user's preferences.
    /// </summary>
    /// <param name="ignoreSoftFiltering">
    /// Ignores prerequisite and progression level filtering.
    /// Does not ignore user-ignores or user-owns-equipment filtering.
    /// </param>
    public QueryBuilder WithUser(User user, bool needsDeload = false, bool ignoreSoftFiltering = false)
    {
        UserOptions = new UserOptions(user, Section)
        {
            NeedsDeload = needsDeload,
            IgnoreProgressions = ignoreSoftFiltering,
            IgnorePrerequisites = ignoreSoftFiltering || user.IgnorePrerequisites,
        };

        return WithSelectionOptions(options =>
        {
            options.UniqueExercises = !ignoreSoftFiltering;
        });
    }

    /// <summary>
    /// Filter variations down to have this equipment.
    /// </summary>
    public QueryBuilder WithEquipment(Equipment equipments, Action<EquipmentOptions>? builder = null)
    {
        var options = EquipmentOptions ?? new EquipmentOptions(equipments);
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
        var options = ExerciseOptions ?? new ExerciseOptions(Section);
        builder?.Invoke(options);
        ExerciseOptions = options;
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

    public QueryBuilder WithSkills(Type? skillType, int? skills, Action<SkillsOptions>? builder = null)
    {
        var options = SkillsOptions ?? new SkillsOptions(skillType, skills);
        builder?.Invoke(options);
        SkillsOptions = options;
        return this;
    }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public QueryRunner Build()
    {
        return new QueryRunner(Section)
        {
            UserOptions = UserOptions ?? new UserOptions(),
            SportsOptions = SportsOptions ?? new SportsOptions(),
            SkillsOptions = SkillsOptions ?? new SkillsOptions(),
            ExerciseOptions = ExerciseOptions ?? new ExerciseOptions(),
            ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
            EquipmentOptions = EquipmentOptions ?? new EquipmentOptions(),
            SelectionOptions = SelectionOptions ?? new SelectionOptions(),
            MuscleGroupOptions = MuscleGroupOptions ?? new MuscleGroupOptions(),
            ExerciseFocusOptions = ExerciseFocusOptions ?? new ExerciseFocusOptions(),
            MuscleMovementOptions = MuscleMovementOptions ?? new MuscleMovementOptions(),
            MovementPatternOptions = MovementPatternOptions ?? new MovementPatternOptions(),
        };
    }
}