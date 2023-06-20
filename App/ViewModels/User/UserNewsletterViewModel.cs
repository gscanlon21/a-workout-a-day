using Core.Models.Exercise;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using App.Dtos.User;

namespace App.ViewModels.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterViewModel
{
    /// <summary>
    /// Show/hide content that should only be visible in the demo?
    /// </summary>
    public bool Demo => Features.HasFlag(Features.Demo);

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; init; }

    public string Email { get; init; }

    public string Token { get; init; }

    public Features Features { get; init; }

    [Display(Name = "Footnote Types")]
    public FootnoteType FootnoteType { get; init; }

    public bool ShowStaticImages { get; init; }

    public bool SendMobilityWorkouts { get; init; }

    public DateOnly? LastActive { get; init; }

    [Display(Name = "Mobility Muscles")]
    public MuscleGroups MobilityMuscles { get; set; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; init; }

    [Display(Name = "Send Days")]
    public Days SendDays { get; init; }

    [Display(Name = "Prehab Focus")]
    public PrehabFocus PrehabFocus { get; init; }

    [Display(Name = "Rehab Focus")]
    public RehabFocus RehabFocus { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Email Verbosity")]
    public Verbosity EmailVerbosity { get; init; }

    [Display(Name = "Workout Intensity")]
    public IntensityLevel IntensityLevel { get; init; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    [Display(Name = "Weeks Between Functional Refresh")]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    [Display(Name = "Weeks Between Accessory Refresh")]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    //[JsonIgnore]
    public ICollection<UserExercise> UserExercises { get; init; }

    //[JsonIgnore]
    public ICollection<UserVariation> UserVariations { get; init; }

    public ICollection<UserEquipment> UserEquipments { get; init; }

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (Core.User.Consts.DisableAfterXMonths - 1));
}
