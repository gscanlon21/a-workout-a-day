using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel
    {
        /// <summary>
        /// The number of footnotes to show in the newsletter
        /// </summary>
        public readonly int FootnoteCount = 3;

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Entities.User.User user, Entities.Newsletter.Newsletter newsletter, string token)
        {
            User = new UserNewsletterViewModel(user, token);
            Newsletter = newsletter;
            Exercises = exercises;
            Verbosity = user.EmailVerbosity;
        }

        public UserNewsletterViewModel User { get; }
        public IList<ExerciseViewModel> Exercises { get; }
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
        /// Display which equipment the user does not have.
        /// </summary>
        [UIHint(nameof(Equipment))]
        public EquipmentViewModel AllEquipment { get; init; } = null!;
    }
}
