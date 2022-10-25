using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.ViewModels.Newsletter;

namespace FinerFettle.Web.ViewModels.Exercise
{
    public class ExerciseSectionViewModel
    {
        public ExerciseSectionViewModel(string? title, IList<ExerciseViewModel>? exercises, Verbosity verbosity, ExerciseTheme theme)
            : this(exercises, verbosity, theme)
        {
            Title = title;
        }

        public ExerciseSectionViewModel(IList<ExerciseViewModel>? exercises, Verbosity verbosity, ExerciseTheme theme)
        {
            Verbosity = verbosity;
            Theme = theme;
            Exercises = exercises;
        }

        public string? Title { get; }
        public Verbosity Verbosity { get; }
        public ExerciseTheme Theme { get; }
        public IList<ExerciseViewModel>? Exercises { get; }
    }
}
