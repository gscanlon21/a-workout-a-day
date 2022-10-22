using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class ExercisesViewModel
    {
        public ExercisesViewModel(Verbosity verbosity, IList<ExerciseViewModel> exercises)
        {
            Exercises = exercises;
            Verbosity = verbosity;
        }

        public IList<ExerciseViewModel> Exercises { get; }

        /// <summary>
        /// How much detail to show in the newsletter.
        /// </summary>
        public Verbosity Verbosity { get; }
    }
}
