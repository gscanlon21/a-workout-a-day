using ADay.Core.Models.Footnote;
using Core.Interfaces.User;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using System.Diagnostics;

namespace Core.Dtos.User;

/// <summary>
/// DTO for User.cs
/// </summary>
[DebuggerDisplay("Email = {Email}, LastActive = {LastActive}")]
public class UserDto : IUser
{
    public int Id { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    public string Email { get; init; } = null!;

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    public ImageType ImageType { get; init; }

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    public Equipment Equipment { get; init; }

    /// <summary>
    /// User would like to receive emails on their off days recommending mobility and stretching exercises?
    /// </summary>
    public bool IncludeMobilityWorkouts { get; init; }

    /// <summary>
    /// User is new to fitness?
    /// </summary>
    public bool IsNewToFitness => SeasonedDate == null;

    /// <summary>
    /// When was the user no longer new to fitness. 
    /// Or null if the user is still new to fitness.
    /// 
    /// Date is UTC.
    /// </summary>
    public DateOnly? SeasonedDate { get; init; }

    /// <summary>
    /// Types of footnotes to show to the user.
    /// </summary>
    public FootnoteType FootnoteType { get; init; }

    /// <summary>
    /// Focus areas to work on while on off days.
    /// </summary>
    public PrehabFocus PrehabFocus { get; init; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises.
    /// </summary>
    public RehabFocus RehabFocus { get; init; }

    /// <summary>
    /// Include a section to boost a specific sports performance.
    /// </summary>
    public SportsFocus SportsFocus { get; init; }

    /// <summary>
    /// Days the user want to send the newsletter.
    /// </summary>
    public Days SendDays { get; init; }

    /// <summary>
    /// How intense the user wants workouts to be.
    /// </summary>
    public Intensity Intensity { get; init; }

    /// <summary>
    /// The user's preferred workout split.
    /// </summary>
    public Frequency Frequency { get; init; }

    /// <summary>
    /// What level of detail the user wants in their newsletter?
    /// </summary>
    public Verbosity Verbosity { get; init; }

    /// <summary>
    /// When was the user last active?
    /// 
    /// Is `null` when the user has not confirmed their account.
    /// </summary>
    public DateOnly? LastActive { get; init; } = null;

    public DateOnly CreatedDate { get; init; }

    /// <summary>
    /// What features should the user have access to?
    /// </summary>
    public Features Features { get; init; } = Features.None;

    public int FontSizeAdjust { get; init; }


    #region Advanced Preferences

    public int FootnoteCountTop { get; init; } = UserConsts.FootnoteCountTopDefault;

    public int FootnoteCountBottom { get; init; } = UserConsts.FootnoteCountBottomDefault;

    #endregion


    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is UserDto other
        && other.Id == Id;
}
