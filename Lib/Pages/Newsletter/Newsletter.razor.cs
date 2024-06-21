using Core.Consts;
using Core.Dtos.Workout;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Lib.Pages.Shared.Exercise;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lib.Pages.Newsletter;


public class NewsletterViewModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DateOnly Today { get; init; }

    public UserNewsletterViewModel User { get; init; } = null!;
    public UserWorkoutViewModel UserWorkout { get; init; } = null!;

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; init; }

    public IList<ExerciseVariationViewModel> Exercises { get; init; } = null!;

    public IList<ExerciseVariationViewModel> MainExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> PrehabExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> RehabExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> WarmupExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> SportsExercises { get; init; } = null!;
    public IList<ExerciseVariationViewModel> CooldownExercises { get; init; } = null!;

    /// <summary>
    /// Hiding the footer in the demo iframe.
    /// </summary>
    public bool HideFooter { get; set; } = false;
}


/// <summary>
/// Model for user_workout db table.
/// </summary>
public class UserWorkoutViewModel
{
    public int Id { get; init; }

    [Required]
    public int UserId { get; init; }

    /// <summary>
    /// The date the newsletter was sent out on
    /// </summary>
    [Required]
    public DateOnly Date { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    [Required]
    public WorkoutRotationViewModel Rotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    [Required]
    public Frequency Frequency { get; init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    [Required]
    public bool IsDeloadWeek { get; init; }
}


/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterViewModel
{
    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; init; }

    public string Email { get; init; } = null!;

    public string Token { get; set; } = null!;

    public Features Features { get; init; }

    [Display(Name = "Footnotes")]
    public FootnoteType FootnoteType { get; init; }

    public bool ShowStaticImages { get; init; }

    public bool IncludeMobilityWorkouts { get; init; }

    public DateOnly? LastActive { get; init; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; init; }

    [Display(Name = "Strengthening Days")]
    public Days SendDays { get; init; }

    [Display(Name = "Prehab Focus")]
    public PrehabFocus PrehabFocus { get; init; }

    [Display(Name = "Rehab Focus")]
    public RehabFocus RehabFocus { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Workout Intensity")]
    public Intensity Intensity { get; init; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    [Display(Name = "Weeks Between Functional Refresh")]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    [Display(Name = "Weeks Between Accessory Refresh")]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    public Core.Models.Equipment.Equipment Equipment { get; init; }

    [JsonInclude]
    public ICollection<UserExerciseViewModel> UserExercises { get; init; } = null!;

    [JsonInclude]
    public ICollection<UserVariationViewModel> UserVariations { get; init; } = null!;

    public int FootnoteCountTop { get; init; }

    public int FootnoteCountBottom { get; init; }

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
