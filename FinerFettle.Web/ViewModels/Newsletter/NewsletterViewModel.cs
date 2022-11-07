using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter;

public class NewsletterViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 3;

    public NewsletterViewModel(Entities.User.User user, Entities.Newsletter.Newsletter newsletter, string token)
    {
        User = new UserNewsletterViewModel(user, token);
        Newsletter = newsletter;
        Verbosity = user.EmailVerbosity;
    }

    public UserNewsletterViewModel User { get; }
    public IList<ExerciseViewModel> ExtraExercises { get; init; } = null!;
    public IList<ExerciseViewModel> MainExercises { get; init; } = null!;
    public Entities.Newsletter.Newsletter Newsletter { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; private init; }

    /// <summary>
    /// Show/hide content that should only be visible in the demo?
    /// </summary>
    public bool Demo => User.Email == Entities.User.User.DemoUser;

    public IList<ExerciseViewModel>? RecoveryExercises { get; init; }
    public IList<ExerciseViewModel> WarmupExercises { get; init; } = null!;
    public IList<ExerciseViewModel> WarmupCardioExercises { get; init; } = null!;
    public IList<ExerciseViewModel>? SportsExercises { get; init; }
    public IList<ExerciseViewModel> CooldownExercises { get; init; } = null!;
    public IList<ExerciseViewModel>? DebugExercises { get; init; }

    /// <summary>
    /// Exercises to update the last seen date with.
    /// </summary>
    public IEnumerable<ExerciseViewModel> AllExercises => MainExercises
        //.Concat(ExtraExercises)
        .Concat(WarmupExercises)
        .Concat(WarmupCardioExercises)
        .Concat(CooldownExercises)
        .Concat(RecoveryExercises ?? new List<ExerciseViewModel>())
        .Concat(SportsExercises ?? new List<ExerciseViewModel>())
        .Concat(DebugExercises ?? new List<ExerciseViewModel>());

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    [UIHint(nameof(Equipment))]
    public EquipmentViewModel AllEquipment { get; init; } = null!;
}
