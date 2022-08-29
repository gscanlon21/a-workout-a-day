using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
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

        public NewsletterViewModel(IList<ExerciseViewModel> exercises, Models.User.User user)
        {
            Exercises = exercises;
            Verbosity = user.EmailVerbosity;
            User = new UserNewsletterViewModel(user);
        }

        public UserNewsletterViewModel? User { get; }

        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel> Exercises { get; init; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        public ExerciseType ExerciseType { get; init; }
        public MuscleGroups? MuscleGroups { get; init; }

        public bool Demo { get; init; }
        public Verbosity Verbosity { get; }
    }
}
