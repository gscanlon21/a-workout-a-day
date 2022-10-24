using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;
using FinerFettle.Web.ViewModels.User;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class ExerciseSectionViewModel
    {
        public ExerciseSectionViewModel(string? title, IList<ExerciseViewModel>? exercises, Verbosity verbosity, ExerciseActivityLevel activityLevel)
            : this(exercises, verbosity, activityLevel)
        {
            Title = title;
        }

        public ExerciseSectionViewModel(IList<ExerciseViewModel>? exercises, Verbosity verbosity, ExerciseActivityLevel activityLevel)
        {
            Verbosity = verbosity;
            ActivityLevel = activityLevel;
            Exercises = exercises;
        }

        public string? Title { get; }
        public Verbosity Verbosity { get; }
        public ExerciseActivityLevel ActivityLevel { get; }
        public IList<ExerciseViewModel>? Exercises { get; }
    }
}
