using Core.Consts;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// User's preferences for use in the newsletter.
/// </summary>
public class UserNewsletterDto
{
    [Obsolete("Public parameterless constructor required for model binding.", error: true)]
    public UserNewsletterDto() { }

    public UserNewsletterDto(UserDto user, string token)
    {
        Token = token;
        Email = user.Email;
        Features = user.Features;
        SendDays = user.SendDays;
        ImageType = user.ImageType;
        Equipment = user.Equipment;
        Verbosity = user.Verbosity;
        Intensity = user.Intensity;
        Frequency = user.Frequency;
        LastActive = user.LastActive;
        RehabFocus = user.RehabFocus;
        PrehabFocus = user.PrehabFocus;
        SportsFocus = user.SportsFocus;
        FootnoteType = user.FootnoteType;
        IsNewToFitness = user.IsNewToFitness;
        FootnoteCountTop = user.FootnoteCountTop;
        FootnoteCountBottom = user.FootnoteCountBottom;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
    }

    public UserNewsletterDto(UserDto user, string token, TimeSpan timeUntilDeload) : this(user, token)
    {
        TimeUntilDeload = timeUntilDeload;
    }

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public string Email { get; init; } = null!;

    public string Token { get; set; } = null!;

    public Features Features { get; init; }

    public Equipment Equipment { get; init; }

    [Display(Name = "Footnotes")]
    public FootnoteType FootnoteType { get; init; }

    public ImageType ImageType { get; init; }

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

    public int FootnoteCountTop { get; init; }

    public int FootnoteCountBottom { get; init; }

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
