using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Code.Exceptions;
using Data.Query.Builders.MuscleGroup;
using Data.Query.Options;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public abstract class QueryBuilderBase
{
    protected readonly Section Section;

    protected SportsOptions? SportsOptions;
    protected SkillsOptions? SkillsOptions;
    protected ExerciseOptions? ExerciseOptions;
    protected ExclusionOptions? ExclusionOptions;
    protected EquipmentOptions? EquipmentOptions;
    protected SelectionOptions? SelectionOptions;
    protected MuscleGroupOptions? MuscleGroupOptions;
    protected ExerciseFocusOptions? ExerciseFocusOptions;
    protected MuscleMovementOptions? MuscleMovementOptions;
    protected MovementPatternOptions? MovementPatternOptions;

    /// <summary>
    /// Looks for similar buckets of exercise variations.
    /// </summary>
    public QueryBuilderBase(Section section)
    {
        Section = section;
    }

    /// <summary>
    /// Filter exercises down to the specified type.
    /// </summary>
    /// <param name="value">Will filter down to any of the flags values.</param>
    public virtual QueryBuilderBase WithExerciseFocus(IList<ExerciseFocus> value, Action<ExerciseFocusOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(ExerciseFocusOptions);
        ExerciseFocusOptions ??= new ExerciseFocusOptions(value);
        builder?.Invoke(ExerciseFocusOptions);
        return this;
    }

    /// <summary>
    /// What progression level should we cap exercise's at?
    /// </summary>
    public virtual QueryBuilderBase WithSelectionOptions(Action<SelectionOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SelectionOptions);
        SelectionOptions ??= new SelectionOptions();
        builder?.Invoke(SelectionOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public virtual QueryBuilderBase WithMuscleMovement(MuscleMovement muscleMovement, Action<MuscleMovementOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MuscleMovementOptions);
        MuscleMovementOptions ??= new MuscleMovementOptions(muscleMovement);
        builder?.Invoke(MuscleMovementOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to these muscle movement.
    /// </summary>
    public virtual QueryBuilderBase WithMovementPatterns(MovementPattern movementPatterns, Action<MovementPatternOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MovementPatternOptions);
        MovementPatternOptions ??= new MovementPatternOptions(movementPatterns);
        builder?.Invoke(MovementPatternOptions);
        return this;
    }

    /// <summary>
    /// Show exercises that work these unique muscle groups.
    /// </summary>
    public virtual QueryBuilderBase WithMuscleGroups(IMuscleGroupBuilder builder, Action<MuscleGroupOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(MuscleGroupOptions);
        MuscleGroupOptions ??= builder.Build(Section);
        optionsBuilder?.Invoke(MuscleGroupOptions);
        return this;
    }

    /// <summary>
    /// Filter variations down to have this equipment.
    /// </summary>
    public virtual QueryBuilderBase WithEquipment(Equipment equipments, Action<EquipmentOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(EquipmentOptions);
        EquipmentOptions ??= new EquipmentOptions(equipments);
        builder?.Invoke(EquipmentOptions);
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public virtual QueryBuilderBase WithExcludeExercises(Action<ExclusionOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(ExclusionOptions);
        ExclusionOptions ??= new ExclusionOptions();
        builder?.Invoke(ExclusionOptions);
        return this;
    }

    /// <summary>
    /// The exercise ids and not the variation or exercisevariation ids.
    /// </summary>
    public virtual QueryBuilderBase WithExercises(Action<ExerciseOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(ExerciseOptions);
        ExerciseOptions ??= new ExerciseOptions(Section);
        builder?.Invoke(ExerciseOptions);
        return this;
    }

    /// <summary>
    /// Filter variations to the ones that target this sport.
    /// </summary>
    public virtual QueryBuilderBase WithSportsFocus(SportsFocus sportsFocus, Action<SportsOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SportsOptions);
        SportsOptions ??= new SportsOptions(sportsFocus);
        builder?.Invoke(SportsOptions);
        return this;
    }

    public virtual QueryBuilderBase WithSkills(Type? skillType, int? skills, Action<SkillsOptions>? builder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(SkillsOptions);
        SkillsOptions ??= new SkillsOptions(skillType, skills);
        builder?.Invoke(SkillsOptions);
        return this;
    }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public abstract QueryRunner Build();
}