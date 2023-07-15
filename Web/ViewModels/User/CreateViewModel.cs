namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class CreateViewModel
{
    public const string EmailRegex = @"\s*\S+@\S+\.\S+\s*";
    public const string EmailRegexError = "Please enter a valid email address.";

    public CreateViewModel()
    {
        UserCreateViewModel = new UserCreateViewModel()
        {
            IsNewToFitness = true
        };
    }

    public CreateViewModel(Lib.ViewModels.User.UserViewModel user, string token)
    {
        UserCreateViewModel = new UserCreateViewModel()
        {
            Email = user.Email,
            AcceptedTerms = user.AcceptedTerms,
            IsNewToFitness = user.IsNewToFitness,
            Token = token,
        };
    }

    public UserCreateViewModel UserCreateViewModel { get; set; } = new UserCreateViewModel();
    public UserLoginViewModel UserLoginViewModel { get; set; } = new UserLoginViewModel();

    /// <summary>
    /// If null, user has not yet tried to subscribe.
    /// If true, user has successfully subscribed.
    /// If false, user failed to subscribe.
    /// </summary>
    public bool? WasSubscribed { get; set; }

    /// <summary>
    /// If null, user has not yet tried to unsubscribe.
    /// If true, user has successfully unsubscribed.
    /// If false, user failed to unsubscribe.
    /// </summary>
    public bool? WasUnsubscribed { get; set; }
}
