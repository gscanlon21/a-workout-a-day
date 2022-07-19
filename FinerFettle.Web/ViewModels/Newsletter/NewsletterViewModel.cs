using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel
    {
        public NewsletterViewModel() : this(null) { }
        public NewsletterViewModel(User? user)
        {
            User = user;
        }

        /// <summary>
        /// The number of footnotes to show in the newsletter
        /// </summary>
        public readonly int FootnoteCount = 3;

        public User? User { get; init; }
        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel>? Exercises { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public MuscleGroups? MuscleGroups { get; set; }
    }
}
