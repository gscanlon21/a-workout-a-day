using Core.Models.Newsletter;
using Data.Query.Options;
using Data.Query.Options.Users;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class QueryBuilder : QueryBuilderBase
{
    public QueryBuilder(Section section) : base(section) { }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public override QueryRunner Build()
    {
        return new QueryRunner(Section)
        {
            UserOptions = new UserOptions(),
            UserIgnoreOptions = new UserIgnoreOptions(),
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