using Core.Models.Newsletter;
using Lib.ViewModels.User;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Debug.cshtml
/// </summary>
public class DebugViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DebugViewModel(User.UserViewModel user, string token)
    {
        //User = new UserNewsletterViewModel(user, token);
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterViewModel User { get; } = null!;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    public required IList<ExerciseViewModel> DebugExercises { get; init; }

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    public EquipmentViewModel Equipment { get; init; } = null!;
}
