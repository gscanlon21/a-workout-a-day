using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class ExercisesViewModel
    {
        public ExercisesViewModel(IList<ExerciseViewModel> exercises, Verbosity verbosity)
        {
            Exercises = exercises;
            Verbosity = verbosity;
        }

        public IList<ExerciseViewModel> Exercises { get; set; }

        /// <summary>
        /// How much detail to show in the newsletter.
        /// </summary>
        public Verbosity Verbosity { get; }
    }
}
