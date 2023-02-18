using System.ComponentModel.DataAnnotations;
using Web.Entities.Equipment;
using Web.Models.Newsletter;
using Web.ViewModels.User;

namespace Web.ViewModels.Newsletter;

public class DebugViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DebugViewModel(Entities.User.User user, string token)
    {
        User = new UserNewsletterViewModel(user, token);
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterViewModel User { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; private init; }

    /// <summary>
    /// Show/hide content that should only be visible in the demo?
    /// </summary>
    public bool Demo => User.Email == Entities.User.User.DemoUser;

    public IList<ExerciseViewModel> DebugExercises { get; init; }


    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    [Display(Name = "Days Until Functional Refresh")]
    public TimeSpan TimeUntilFunctionalRefresh { get; set; } = TimeSpan.Zero;

    [Display(Name = "Days Until Accessory Refresh")]
    public TimeSpan TimeUntilAccessoryRefresh { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    [UIHint(nameof(Equipment))]
    public EquipmentViewModel AllEquipment { get; init; } = null!;
}
