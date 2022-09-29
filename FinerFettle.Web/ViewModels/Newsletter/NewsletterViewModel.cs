using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.User;

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

        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel> Exercises { get; init; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        /// <summary>
        /// What exercise type is the workout today targeting>
        /// </summary>
        public ExerciseType ExerciseType { get; init; }

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
    }
}
