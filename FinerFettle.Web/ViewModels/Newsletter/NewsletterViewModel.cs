using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Exercise;
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

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Models.User.User user, Models.Newsletter.Newsletter newsletter, string token)
        {
            User = new UserNewsletterViewModel(user, token);
            Newsletter = newsletter;
            Exercises = exercises;
            Verbosity = user.EmailVerbosity;
        }

        public IList<ExerciseViewModel> Exercises { get; }

        /// <summary>
        /// How much detail to show in the newsletter.
        /// </summary>
        public Verbosity Verbosity { get; }

        public UserNewsletterViewModel User { get; }
        public Models.Newsletter.Newsletter Newsletter { get; }

        /// <summary>
        /// Show/hide content that should only be visible in the demo?
        /// </summary>
        public bool Demo => User.Email == Models.User.User.DemoUser;

        public IList<ExerciseViewModel>? RecoveryExercises { get; set; }
        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel>? SportsExercises { get; set; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        /// <summary>
        /// Display which equipment the user does not have.
        /// </summary>
        [UIHint(nameof(Equipment))]
        public EquipmentViewModel AllEquipment { get; init; } = null!;
    }
}
