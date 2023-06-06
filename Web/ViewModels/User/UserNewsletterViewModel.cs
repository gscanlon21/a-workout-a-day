using System.ComponentModel.DataAnnotations;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Footnote;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.ViewModels.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterViewModel
{
    public UserNewsletterViewModel(Entities.User.User user, string token)
    {
        Id = user.Id;
        Email = user.Email;
        SendMobilityWorkouts = user.SendMobilityWorkouts;
        MobilityMuscles = user.MobilityMuscles;
        PrehabFocus = user.PrehabFocus;
        RehabFocus = user.RehabFocus;
        SendDays = user.SendDays;
        UserEquipments = user.UserEquipments;
        IntensityLevel = user.IntensityLevel;
        Frequency = user.Frequency;
        PreferStaticImages = user.PreferStaticImages;
        IsNewToFitness = user.IsNewToFitness;
        UserExercises = user.UserExercises;
        UserVariations = user.UserVariations;
        SportsFocus = user.SportsFocus;
        LastActive = user.LastActive;
        RefreshFunctionalEveryXWeeks = user.RefreshFunctionalEveryXWeeks;
        RefreshAccessoryEveryXWeeks = user.RefreshAccessoryEveryXWeeks;
        EmailVerbosity = user.EmailVerbosity;
        FootnoteType = user.FootnoteType;
        Features = user.Features;
        Token = token;
    }

    /// <summary>
    /// Show/hide content that should only be visible in the demo?
    /// </summary>
    public bool Demo => Features.HasFlag(Features.Demo);

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; }

    public string Email { get; }

    public string Token { get; }

    public Features Features { get; }

    [Display(Name = "Footnote Types")]
    public FootnoteType FootnoteType { get; }

    public bool PreferStaticImages { get; }

    public bool SendMobilityWorkouts { get; }

    public DateOnly? LastActive { get; }

    [Display(Name = "Mobility Muscles")]
    public MuscleGroups MobilityMuscles { get; set; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; }

    [Display(Name = "Send Days")]
    public Days SendDays { get; }

    [Display(Name = "Prehab Focus")]
    public PrehabFocus PrehabFocus { get; }

    [Display(Name = "Rehab Focus")]
    public RehabFocus RehabFocus { get; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Email Verbosity")]
    public Verbosity EmailVerbosity { get; }

    [Display(Name = "Workout Intensity")]
    public IntensityLevel IntensityLevel { get; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; }

    [Display(Name = "Weeks Between Functional Refresh")]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    [Display(Name = "Weeks Between Accessory Refresh")]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    public ICollection<UserExercise> UserExercises { get; init; }

    public ICollection<UserVariation> UserVariations { get; init; }

    public ICollection<UserEquipment> UserEquipments { get; init; }

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (Core.User.Consts.DisableAfterXMonths - 1));
}
