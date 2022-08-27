using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class NewsletterViewModel
    {
        /// <summary>
        /// The number of footnotes to show in the newsletter
        /// </summary>
        public readonly int FootnoteCount = 3;

        public NewsletterViewModel(IList<ExerciseViewModel> exercises)
        {
            Exercises = exercises;
        }

        public Models.User.User? User { get; init; }

        public IList<ExerciseViewModel>? WarmupExercises { get; set; }
        public IList<ExerciseViewModel> Exercises { get; init; }
        public IList<ExerciseViewModel>? CooldownExercises { get; set; }

        public ExerciseType ExerciseType { get; init; }
        public MuscleGroups? MuscleGroups { get; init; }

        public bool Demo { get; init; }
        public bool Verbose { get; init; }
    }
}
