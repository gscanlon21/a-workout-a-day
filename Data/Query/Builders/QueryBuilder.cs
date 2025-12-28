using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Code.Exceptions;
using Data.Entities.Users;
using Data.Query.Builders.MuscleGroup;
using Data.Query.Options;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class QueryBuilder
{
    private readonly Section Section;

    private UserOptions? UserOptions;
    private SportsOptions? SportsOptions;
    private SkillsOptions? SkillsOptions;
    private ExerciseOptions? ExerciseOptions;
    private ExclusionOptions? ExclusionOptions;
    private EquipmentOptions? EquipmentOptions;
    private SelectionOptions? SelectionOptions;
    private MuscleGroupOptions? MuscleGroupOptions;
    private ExerciseFocusOptions? ExerciseFocusOptions;
    private MuscleMovementOptions? MuscleMovementOptions;
    private MovementPatternOptions? MovementPatternOptions;

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
        InvalidOptionsException.ThrowIfAlreadySet(ExerciseFocusOptions);
        ExerciseFocusOptions ??= new ExerciseFocusOptions(value);
        builder?.Invoke(ExerciseFocusOptions);
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public QueryBuilder WithSelectionOptions(Action<SelectionOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SelectionOptions);
        SelectionOptions ??= new SelectionOptions();
        builder?.Invoke(SelectionOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMuscleMovement(MuscleMovement muscleMovement, Action<MuscleMovementOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MuscleMovementOptions);
        MuscleMovementOptions ??= new MuscleMovementOptions(muscleMovement);
        builder?.Invoke(MuscleMovementOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public QueryBuilder WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MovementPatternOptions);
        MovementPatternOptions ??= new MovementPatternOptions(movementPatterns);
        builder?.Invoke(MovementPatternOptions);
        return this;
    }

    /// <summary>
    /// Show exercises that work these unique muscle groups.
    /// </summary>
    public QueryBuilder WithMuscleGroups(IMuscleGroupBuilder builder, Action<MuscleGroupOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MuscleGroupOptions);
        MuscleGroupOptions ??= builder.Build(Section);
        optionsBuilder?.Invoke(MuscleGroupOptions);
        return this;
    }

    /// <summary>
    /// Filter variations according to the user's preferences.
    /// </summary>
    public QueryBuilder WithUser(User user, Action<UserOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(UserOptions);
        UserOptions ??= new UserOptions(user, Section);
        optionsBuilder?.Invoke(UserOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment.
    /// </summary>
    public QueryBuilder WithEquipment(Equipment equipments, Action<EquipmentOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(EquipmentOptions);
        EquipmentOptions ??= new EquipmentOptions(equipments);
        builder?.Invoke(EquipmentOptions);
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExcludeExercises(Action<ExclusionOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(ExclusionOptions);
        ExclusionOptions ??= new ExclusionOptions();
        builder?.Invoke(ExclusionOptions);
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public QueryBuilder WithExercises(Action<ExerciseOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(ExerciseOptions);
        ExerciseOptions ??= new ExerciseOptions(Section);
        builder?.Invoke(ExerciseOptions);
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this sport.
    /// </summary>
    public QueryBuilder WithSportsFocus(SportsFocus sportsFocus, Action<SportsOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SportsOptions);
        SportsOptions ??= new SportsOptions(sportsFocus);
        builder?.Invoke(SportsOptions);
        return this;
    }

    public QueryBuilder WithSkills(Type? skillType, int? skills, Action<SkillsOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SkillsOptions);
        SkillsOptions ??= new SkillsOptions(skillType, skills);
        builder?.Invoke(SkillsOptions);
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