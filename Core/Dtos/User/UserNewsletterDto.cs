using ADay.Core.Models.Footnote;
using Core.Code.Helpers;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;

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
        CreatedDate = user.CreatedDate;
        PrehabFocus = user.PrehabFocus;
        SportsFocus = user.SportsFocus;
        FootnoteType = user.FootnoteType;
        FontSizeAdjust = user.FontSizeAdjust;
        IsNewToFitness = user.IsNewToFitness;
        FootnoteCountTop = user.FootnoteCountTop;
        FootnoteCountBottom = user.FootnoteCountBottom;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
    }

    public UserNewsletterDto(UserDto user, string token, TimeSpan timeUntilDeload) : this(user, token)
    {
        TimeUntilDeload = timeUntilDeload;
    }

    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public string Email { get; init; } = null!;

    public string Token { get; set; } = null!;

    public Features Features { get; init; }

    public Equipment Equipment { get; init; }

    public FootnoteType FootnoteType { get; init; }

    public ImageType ImageType { get; init; }

    public bool IsNewToFitness { get; init; }

    public bool IncludeMobilityWorkouts { get; init; }

    public DateOnly CreatedDate { get; init; }

    public DateOnly? LastActive { get; init; }

    public Days SendDays { get; init; }

    public RehabFocus RehabFocus { get; init; }

    public PrehabFocus PrehabFocus { get; init; }

    public SportsFocus SportsFocus { get; init; }

    public Verbosity Verbosity { get; init; }

    public Intensity Intensity { get; init; }

    public Frequency Frequency { get; init; }

    public int FontSizeAdjust { get; init; }

    public int FootnoteCountTop { get; init; }

    public int FootnoteCountBottom { get; init; }

    public bool IsNewlyCreated => CreatedDate >= DateHelpers.Today.AddDays(-7);

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateHelpers.Today.AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
