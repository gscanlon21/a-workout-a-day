using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;

using System.Diagnostics;
using System.Numerics;

namespace Lib.ViewModels.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[DebuggerDisplay("Email = {Email}, Disabled = {Disabled}")]
public class UserViewModel
{
    public int Id { get; init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    public string Email { get; init; } = null!;

    /// <summary>
    /// User has accepted the current Terms of Use when they signed up.
    /// </summary>
    [Required]
    public bool AcceptedTerms { get; init; }

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    [Required]
    public bool ShowStaticImages { get; set; }


    /// <summary>
    /// User would like emails on their off days recommending mobility and stretching exercises?
    /// </summary>
    [Required]
    public bool IncludeMobilityWorkouts { get; set; }

    /// <summary>
    /// User is new to fitness?
    /// </summary>
    public bool IsNewToFitness
    {
        get => SeasonedDate == null;
        set
        {
            if (SeasonedDate == null && !value)
            {
                SeasonedDate = DateOnly.FromDateTime(DateTime.UtcNow);
            }
        }
    }

    /// <summary>
    /// User is new to fitness?
    /// 
    /// Naming is hard.
    /// </summary>
    public DateOnly? SeasonedDate { get; set; }

    /// <summary>
    /// Types of footnotes to show to the user.
    /// </summary>
    [Required]
    public FootnoteType FootnoteType { get; set; }

    /// <summary>
    /// Focus areas to work on while on off days.
    /// </summary>
    [Required]
    public PrehabFocus PrehabFocus { get; set; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises.
    /// </summary>
    [Required]
    public RehabFocus RehabFocus { get; set; }

    /// <summary>
    /// Include a section to boost a specific sports performance.
    /// </summary>
    [Required]
    public SportsFocus SportsFocus { get; set; }

    /// <summary>
    /// Days the user want to skip the newsletter.
    /// </summary>
    public Days RestDays => Days.All & ~SendDays;

    /// <summary>
    /// Days the user want to send the newsletter.
    /// </summary>
    [Required]
    public Days SendDays { get; set; }

    /// <summary>
    /// What hour of the day (UTC) should we send emails to this user.
    /// </summary>
    [Required, Range(0, 23)]
    public int SendHour { get; set; }

    /// <summary>
    /// Whan this user was created.
    /// </summary>
    [Required]
    public DateOnly CreatedDate { get; init; }

    /// <summary>
    /// How intense the user wants workouts to be.
    /// </summary>
    [Required]
    public IntensityLevel IntensityLevel { get; set; }

    /// <summary>
    /// The user's preferred workout split.
    /// </summary>
    [Required]
    public Frequency Frequency { get; set; }

    /// <summary>
    /// How often should we show a deload week to the user?
    /// </summary>
    [Required, Range(UserConsts.DeloadAfterEveryXWeeksMin, UserConsts.DeloadAfterEveryXWeeksMax)]
    public int DeloadAfterEveryXWeeks { get; set; }

    /// <summary>
    /// How often to refresh functional movement exercises.
    /// </summary>
    [Required, Range(UserConsts.RefreshFunctionalEveryXWeeksMin, UserConsts.RefreshFunctionalEveryXWeeksMax)]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    /// <summary>
    /// How often to refresh accessory exercises.
    /// </summary>
    [Required, Range(UserConsts.RefreshAccessoryEveryXWeeksMin, UserConsts.RefreshAccessoryEveryXWeeksMax)]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    /// <summary>
    /// What level of detail the user wants in their newsletter?
    /// </summary>
    [Required]
    public Verbosity EmailVerbosity { get; set; }

    /// <summary>
    /// When was the user last active?
    /// </summary>
    public DateOnly? LastActive { get; set; } = null;

    public string? DisabledReason { get; set; } = null;

    /// <summary>
    /// What features should the user have access to?
    /// </summary>
    public Features Features { get; set; } = Features.None;


    #region NotMapped

    public bool Disabled => DisabledReason != null;

    /// <summary>
    /// How many days of the week is the user working out?
    /// </summary>
    public int WorkoutsDays => BitOperations.PopCount((ulong)SendDays);

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId) ?? new List<int>();

    #endregion


    #region Navigation Properties

    //[JsonIgnore, InverseProperty(nameof(UserEquipment.User))]
    public virtual ICollection<UserEquipmentViewModel> UserEquipments { get; init; } = new List<UserEquipmentViewModel>();

    //[JsonIgnore, InverseProperty(nameof(UserMuscle.User))]
    public virtual ICollection<UserMuscleViewModel> UserMuscles { get; init; } = new List<UserMuscleViewModel>();

    //[JsonIgnore, InverseProperty(nameof(UserFrequency.User))]
    public virtual ICollection<UserFrequencyViewModel> UserFrequencies { get; init; } = new List<UserFrequencyViewModel>();

    //[JsonIgnore, InverseProperty(nameof(UserToken.User))]
    public virtual ICollection<UserTokenViewModel> UserTokens { get; init; } = new List<UserTokenViewModel>();

    //[JsonIgnore, InverseProperty(nameof(UserExercise.User))]
    public virtual ICollection<UserExerciseViewModel> UserExercises { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserVariation.User))]
    public virtual ICollection<UserVariationViewModel> UserVariations { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserExerciseVariation.User))]
    public virtual ICollection<UserExerciseVariationViewModel> UserExerciseVariations { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(Newsletter.Newsletter.User))]
    public virtual ICollection<Newsletter.NewsletterEntityViewModel> Newsletters { get; init; } = null!;

    #endregion
}
