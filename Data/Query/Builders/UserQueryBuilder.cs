using Core.Models.Newsletter;
using Data.Code.Exceptions;
using Data.Entities.Users;
using Data.Query.Filters;
using Data.Query.Options;
using Data.Query.Options.Users;
using Data.Query.Runners;

namespace Data.Query.Builders;

/// <summary>
/// Builds out the QueryRunner class with option customization.
/// </summary>
public class UserQueryBuilder<TFilter> : BaseQueryBuilder<UserQueryBuilder<TFilter>>
    where TFilter : BaseQueryFilter
{
    private readonly User User;

    public UserOptions? UserOptions { get; private set; }
    public UserIgnoreOptions? UserIgnoreOptions { get; private set; }

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
    public UserQueryBuilder<TFilter> WithUser(Action<UserOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(UserOptions);
        UserOptions ??= new UserOptions(User);
        optionsBuilder?.Invoke(UserOptions);
        return this;
    }

    /// <summary>
    /// Filter variations according to the user's preferences.
    /// Sets the other relevant user preference options as well.
    /// </summary>
    public UserQueryBuilder<TFilter> WithUserIgnore(Action<UserIgnoreOptions>? optionsBuilder = null)
    {
        InvalidOptionsException.ThrowIfAlreadySet(UserIgnoreOptions);
        UserIgnoreOptions ??= new UserIgnoreOptions(User);
        optionsBuilder?.Invoke(UserIgnoreOptions);
        return this;
    }

    /// <summary>
    /// Builds and returns the QueryRunner class with the options selected.
    /// </summary>
    public override BaseQueryRunner Build()
    {
        return new UserQueryRunner(Section)
        {
            SportsOptions = SportsOptions ?? new SportsOptions(),
            SkillsOptions = SkillsOptions ?? new SkillsOptions(),
            ExerciseOptions = ExerciseOptions ?? new ExerciseOptions(),
            SelectionOptions = SelectionOptions ?? new SelectionOptions(),
            ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
            EquipmentOptions = EquipmentOptions ?? new EquipmentOptions(),
            MuscleGroupOptions = MuscleGroupOptions ?? new MuscleGroupOptions(),
            ExerciseFocusOptions = ExerciseFocusOptions ?? new ExerciseFocusOptions(),
            MuscleMovementOptions = MuscleMovementOptions ?? new MuscleMovementOptions(),
            MovementPatternOptions = MovementPatternOptions ?? new MovementPatternOptions(),
            UserIgnoreOptions = UserIgnoreOptions ?? new UserIgnoreOptions(User),
            UserOptions = UserOptions ?? new UserOptions(User),
            QueryFilter = CreateFilter(),
        };
    }

    private BaseQueryFilter CreateFilter()
    {
        if (typeof(TFilter) == typeof(UserQueryFilter))
        {
            return new UserQueryFilter(Section)
            {
                UserOptions = UserOptions ?? new UserOptions(User),
                SelectionOptions = SelectionOptions ?? new SelectionOptions(),
                ExclusionOptions = ExclusionOptions ?? new ExclusionOptions(),
                MuscleGroupOptions = MuscleGroupOptions ?? new MuscleGroupOptions(),
                MovementPatternOptions = MovementPatternOptions ?? new MovementPatternOptions(),
            };
        }

        if (typeof(TFilter) == typeof(ExerciseQueryFilter))
        {
            return new ExerciseQueryFilter(Section)
            {
                UserOptions = UserOptions ?? new UserOptions(User),
                ExerciseOptions = ExerciseOptions ?? new ExerciseOptions(),
                SelectionOptions = SelectionOptions ?? new SelectionOptions(),
            };
        }

        throw new NotImplementedException();
    }
}