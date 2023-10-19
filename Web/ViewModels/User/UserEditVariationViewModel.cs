using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using Lib.ViewModels.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageVariationViewModel
{
    [Display(Name = "Exercise", Description = "Ignore this exercise and all of its variations.")]
    public required Data.Entities.Exercise.Exercise Exercise { get; init; }

    [Display(Name = "Variation", Description = "Ignore this variation for all of its exercise types.")]
    public required Variation Variation { get; init; }

    [Display(Name = "Exercise Refreshes After", Description = "Refresh this exercise—the next workout will try and select a new exercise if available.")]
    public required UserExercise UserExercise { get; init; }

    [Display(Name = "Variation Refreshes After", Description = "Refresh this variation—the next workout will try and select a new exercise variation if available.")]
    public required UserVariation UserVariation { get; init; }

    public bool? WasUpdated { get; init; }

    public required Section Section { get; init; }
    public required string Email { get; init; }
    public required string Token { get; init; }

    public Verbosity VariationVerbosity => Verbosity.Instructions | Verbosity.Images;
    public Verbosity ExerciseVerbosity => Verbosity.Instructions | Verbosity.Images | Verbosity.ProgressionBar;

    public required UserNewsletterViewModel UserNewsletter { get; init; } = null!;

    public required IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> Exercises { get; init; } = null!;
    public required IList<Lib.ViewModels.Newsletter.ExerciseVariationViewModel> Variations { get; init; } = null!;

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserManageVariationViewModel() { }

    public UserManageVariationViewModel(IList<UserVariationWeight> userWeights, int currentWeight)
    {
        // Skip today, start at 1, because we append the current weight onto the end regardless.
        Xys = Enumerable.Range(1, 365).Select(i =>
        {
            var date = Today.AddDays(-i);
            return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Weight);
        }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(Today, currentWeight)).ToList();
    }

    public int ExerciseId { get; init; }
    public int VariationId { get; init; }

    [Display(Name = "Variation")]
    public string? VariationName { get; set; } = null!;

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(0, 999)]
    [Display(Name = "How much weight are you able to lift?")]
    public int Weight { get; init; }

    internal IList<Xy> Xys { get; init; } = new List<Xy>();

    /// <summary>
    /// For chart.js
    /// </summary>
    internal record Xy(string X, int? Y)
    {
        internal Xy(DateOnly x, int? y) : this(x.ToString("O"), y) { }
    }
}
