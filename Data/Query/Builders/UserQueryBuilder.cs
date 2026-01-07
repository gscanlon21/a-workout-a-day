using Core.Models.Newsletter;
using Data.Code.Exceptions;
using Data.Entities.Users;
using Data.Query.Options;
using Data.Query.Options.Users;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class UserQueryBuilder : QueryBuilderBase
{
    private readonly User User;

    private UserOptions? UserOptions;
    private UserIgnoreOptions? UserIgnoreOptions;

    /// <summary>
    /// Looks for similar buckets of exercise variations.
    /// </summary>
    public UserQueryBuilder(User user, Section section) : base(section)
    {
        User = user;
    }

    /// <summary>
    /// Filter variations according to the user's preferences.
    /// Sets the other relevant user preference options as well.
    /// </summary>
    public UserQueryBuilder WithUser(Action<UserOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(UserOptions);
        UserOptions ??= new UserOptions(User, Section);
        optionsBuilder?.Invoke(UserOptions);
        return this;
    }

    /// <summary>
    /// Filter variations according to the user's preferences.
    /// Sets the other relevant user preference options as well.
    /// </summary>
    public UserQueryBuilder WithUserIgnore(Action<UserIgnoreOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(UserIgnoreOptions);
        UserIgnoreOptions ??= new UserIgnoreOptions(User);
        optionsBuilder?.Invoke(UserIgnoreOptions);
        return this;
    }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public override QueryRunner Build()
    {
        return new QueryRunner(Section)
        {
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
            UserIgnoreOptions = UserIgnoreOptions ?? new UserIgnoreOptions(User),
            UserOptions = UserOptions ?? new UserOptions(User, Section),
        };
    }
}