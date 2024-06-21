using Core.Consts;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterDto
{
    public UserNewsletterDto() { }

    public UserNewsletterDto(UserDto user, string token)
    {
        Id = user.Id;
        Email = user.Email;
        Features = user.Features;
        Equipment = user.Equipment;
        FootnoteType = user.FootnoteType;
        ShowStaticImages = user.ShowStaticImages;
        LastActive = user.LastActive;
        SendDays = user.SendDays;
        PrehabFocus = user.PrehabFocus;
        RehabFocus = user.RehabFocus;
        SportsFocus = user.SportsFocus;
        Verbosity = user.Verbosity;
        Intensity = user.Intensity;
        Frequency = user.Frequency;
        UserExercises = user.UserExercises;
        UserVariations = user.UserVariations;
        IsNewToFitness = user.IsNewToFitness;
        FootnoteCountTop = user.FootnoteCountTop;
        FootnoteCountBottom = user.FootnoteCountBottom;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
        Token = token;
    }

    public UserNewsletterDto(WorkoutContext context) : this(context.User, context.Token)
    {
        TimeUntilDeload = context.TimeUntilDeload;
    }

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; init; }

    public string Email { get; init; }

    public string Token { get; set; }

    public Features Features { get; init; }

    public Equipment Equipment { get; init; }

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

    [Display(Name = "Workout Verbosity")]
    public Verbosity Verbosity { get; init; }

    [Display(Name = "Workout Intensity")]
    public Intensity Intensity { get; init; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    //[JsonIgnore]
    public ICollection<UserExerciseDto> UserExercises { get; init; }

    //[JsonIgnore]
    public ICollection<UserVariationDto> UserVariations { get; init; }

    public int FootnoteCountTop { get; init; }

    public int FootnoteCountBottom { get; init; }

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
