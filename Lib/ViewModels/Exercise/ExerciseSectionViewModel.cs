using Core.Models.Exercise;
using Core.Models.Newsletter;
using Lib.ViewModels.Newsletter;

namespace Lib.ViewModels.Exercise;

/// <summary>
/// Viewmodel for _ExerciseSection.cshtml
/// </summary>
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

    public string? Description { get; init; }
}
