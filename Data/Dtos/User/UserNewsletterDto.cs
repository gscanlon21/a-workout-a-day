using Core.Consts;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.User;
using Data.Models.Newsletter;
using System.ComponentModel.DataAnnotations;

namespace Data.Dtos.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterDto
{
    public UserNewsletterDto(Entities.User.User user, string token)
    {
        Id = user.Id;
        Email = user.Email;
        Equipment = user.Equipment;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
        PrehabFocus = user.PrehabFocus;
        RehabFocus = user.RehabFocus;
        SendDays = user.SendDays;
        Intensity = user.Intensity;
        Frequency = user.Frequency;
        ShowStaticImages = user.ShowStaticImages;
        IsNewToFitness = user.IsNewToFitness;
        UserExercises = user.UserExercises;
        UserVariations = user.UserVariations;
        SportsFocus = user.SportsFocus;
        LastActive = user.LastActive;
        RefreshFunctionalEveryXWeeks = user.RefreshFunctionalEveryXWeeks;
        RefreshAccessoryEveryXWeeks = user.RefreshAccessoryEveryXWeeks;
        Verbosity = user.Verbosity;
        FootnoteType = user.FootnoteType;
        Features = user.Features;
        Token = token;
    }

    internal UserNewsletterDto(WorkoutContext context) : this(context.User, context.Token)
    {
        TimeUntilDeload = context.TimeUntilDeload;
    }

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; }

    public string Email { get; }

    public string Token { get; }

    public Features Features { get; }

    public Equipment Equipment { get; }

    [Display(Name = "Footnotes")]
    public FootnoteType FootnoteType { get; }

    public bool ShowStaticImages { get; }

    public bool IncludeMobilityWorkouts { get; }

    public DateOnly? LastActive { get; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; }

    [Display(Name = "Strengthening Days")]
    public Days SendDays { get; }

    [Display(Name = "Prehab Focus")]
    public PrehabFocus PrehabFocus { get; }

    [Display(Name = "Rehab Focus")]
    public RehabFocus RehabFocus { get; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Workout Verbosity")]
    public Verbosity Verbosity { get; }

    [Display(Name = "Workout Intensity")]
    public Intensity Intensity { get; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; }

    [Display(Name = "Weeks Between Functional Refresh")]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    [Display(Name = "Weeks Between Accessory Refresh")]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    //[JsonIgnore]
    public ICollection<UserExercise> UserExercises { get; init; }

    //[JsonIgnore]
    public ICollection<UserVariation> UserVariations { get; init; }

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
