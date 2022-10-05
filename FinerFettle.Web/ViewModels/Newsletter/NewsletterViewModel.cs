using FinerFettle.Web.Models.Exercise;
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

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Verbosity verbosity)
        {
            Exercises = exercises;
            Verbosity = verbosity;
        }

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Models.User.User user, Models.Newsletter.Newsletter newsletter)
        {
            Exercises = exercises;
            Verbosity = user.EmailVerbosity;
            User = new UserNewsletterViewModel(user);
            Newsletter = newsletter;
        }

        public UserNewsletterViewModel? User { get; }
        public Models.Newsletter.Newsletter? Newsletter { get; }

        public IList<ExerciseViewModel>? RecoveryExercises { get; set; }
        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel> Exercises { get; set; }
        public IList<ExerciseViewModel>? SportsExercises { get; set; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        /// <summary>
        /// What exercise type is the workout today targeting>
        /// </summary>
        public ExerciseType ExerciseType { get; set; }

        /// <summary>
        /// What muscle groups is the workout today targeting?
        /// </summary>
        public MuscleGroups MuscleGroups { get; init; }

        /// <summary>
        /// Show/hide content that should only be visible in the demo?
        /// </summary>
        public bool Demo { get; init; }

        /// <summary>
        /// How much detail to show in the newsletter.
        /// </summary>
        public Verbosity Verbosity { get; }

        /// <summary>
        /// Display which equipment the user does not have.
        /// </summary>
        [UIHint(nameof(Equipment))]
        public EquipmentViewModel AllEquipment { get; init; } = null!;
    }
}
