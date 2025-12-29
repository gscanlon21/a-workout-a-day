using Data.Entities.Users;

namespace Data.Query.Options.Users;

public class UserIgnoreOptions : IOptions
{
    public UserIgnoreOptions() { }

    public UserIgnoreOptions(User user)
    {
        UserExercises = true;
        UserVariations = true;
    }

    /// <summary>
    /// Ignore the user's ignored exercises.
    /// </summary>
    public bool UserExercises { get; set; }

    /// <summary>
    /// Ignore the user's ignored variations.
    /// </summary>
    public bool UserVariations { get; set; }

    public bool HasData() => UserExercises || UserVariations;
}
