using Core.Code.Extensions;
using Core.Code.Helpers;
using Core.Consts;
using Core.Dtos.Exercise;
using Core.Interfaces.User;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[Table("user"), Comment("User who signed up for the newsletter")]
[Index(nameof(Email), IsUnique = true)]
[DebuggerDisplay("Email = {Email}, LastActive = {LastActive}")]
public class User : IUser
{
    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public User() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public User(string email, bool acceptedTerms, bool isNewToFitness)
    {
        if (!acceptedTerms)
        {
            throw new ArgumentException("User must accept the Terms of Use.", nameof(acceptedTerms));
        }

        Email = email.Trim();
        AcceptedTerms = acceptedTerms;
        IsNewToFitness = isNewToFitness;

        SendDays = UserConsts.DaysDefault;
        SendHour = UserConsts.SendHourDefault;
        ImageType = UserConsts.ImageTypeDefault;
        Verbosity = UserConsts.VerbosityDefault;
        Frequency = UserConsts.FrequencyDefault;
        Intensity = UserConsts.IntensityDefault;
        FootnoteType = UserConsts.FootnotesDefault;
        DeloadAfterXWeeks = UserConsts.DeloadAfterXWeeksDefault;

        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    public ImageType ImageType { get; set; }

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    [Required]
    public Core.Models.Equipment.Equipment Equipment { get; set; }

    /// <summary>
    /// User would like to receive emails on their off days recommending mobility and stretching exercises?
    /// </summary>
    [Required]
    public bool IncludeMobilityWorkouts { get; set; }

    /// <summary>
    /// User is new to fitness?
    /// </summary>
    [NotMapped]
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
    /// When was the user no longer new to fitness. 
    /// Or null if the user is still new to fitness.
    /// 
    /// Date is UTC.
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
    /// Don't strengthen this muscle group, but do show recovery variations for exercises.
    /// </summary>
    [Required]
    public int RehabSkills { get; set; }

    /// <summary>
    /// Include a section to boost a specific sports performance.
    /// </summary>
    [Required]
    public SportsFocus SportsFocus { get; set; }

    /// <summary>
    /// Days the user want to skip the newsletter.
    /// </summary>
    [NotMapped]
    public Days RestDays => Days.All & ~SendDays;

    /// <summary>
    /// Days the user want to send the newsletter.
    /// </summary>
    [Required]
    public Days SendDays { get; set; }

    /// <summary>
    /// What hour of the day (UTC) should we send emails to this user.
    /// </summary>
    [Required, Range(UserConsts.SendHourMin, UserConsts.SendHourMax)]
    public int SendHour { get; set; }

    /// <summary>
    /// Offset of today taking into account the user's SendHour.
    /// </summary>
    public DateOnly TodayOffset => DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-1 * SendHour));

    /// <summary>
    /// Offset of today taking into account the user's SendHour and SendDay.
    /// </summary>
    public DateOnly StartOfWeekOffset => Features.HasFlag(Features.Debug) ? TodayOffset
        : TodayOffset.AddDays(-1 * WeekdayDifference);

    private int WeekdayDifference => DateHelpers.StartOfWeek.DayOfWeek > TodayOffset.DayOfWeek
        ? 7 - Math.Abs((int)DateHelpers.StartOfWeek.DayOfWeek - (int)TodayOffset.DayOfWeek)
        : Math.Abs((int)TodayOffset.DayOfWeek - (int)DateHelpers.StartOfWeek.DayOfWeek);

    /// <summary>
    /// When this user was created.
    /// </summary>
    [Required]
    public DateOnly CreatedDate { get; init; }

    /// <summary>
    /// How intense the user wants workouts to be.
    /// </summary>
    [Required]
    public Intensity Intensity { get; set; }

    /// <summary>
    /// The user's preferred workout split.
    /// </summary>
    [Required]
    public Frequency Frequency { get; set; }

    [NotMapped]
    public Frequency ActualFrequency
    {
        get
        {
            if (SendDays.HasFlag(DaysExtensions.FromDate(DateOnly.FromDateTime(DateTime.UtcNow))))
            {
                return Frequency;
            }
            else if (IncludeMobilityWorkouts)
            {
                return Frequency.OffDayStretches;
            }

            return Frequency.None;
        }
    }

    /// <summary>
    /// How often should we show a deload week to the user?
    /// </summary>
    [Required, Range(UserConsts.DeloadAfterXWeeksMin, UserConsts.DeloadAfterXWeeksMax)]
    public int DeloadAfterXWeeks { get; set; }

    /// <summary>
    /// What level of detail the user wants in their newsletter?
    /// </summary>
    [Required]
    public Verbosity Verbosity { get; set; }

    /// <summary>
    /// When was the user last active?
    /// 
    /// Is `null` when the user has not confirmed their account.
    /// </summary>
    public DateOnly? LastActive { get; set; } = null;

    /// <summary>
    /// User would like to receive the workouts in emails?
    /// </summary>
    public string? NewsletterDisabledReason { get; set; } = null;

    /// <summary>
    /// What features should the user have access to?
    /// </summary>
    public Features Features { get; set; } = Features.None;


    #region NotMapped

    /// <summary>
    /// Don't use in queries, is not mapped currently.
    /// </summary>
    [NotMapped]
    public bool IsDemoUser => Features.HasFlag(Features.Demo);

    /// <summary>
    /// Don't use in queries, is not mapped currently.
    /// </summary>
    [NotMapped]
    public bool NewsletterEnabled => NewsletterDisabledReason == null;

    /// <summary>
    /// How many days of the week is the user working out?
    /// 
    /// Don't use in queries, is not mapped currently.
    /// </summary>
    [NotMapped]
    public int WorkoutsDays => SendDays.PopCount();

    #endregion
    #region Advanced Preferences

    public bool IgnorePrerequisites { get; set; }

    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityMin, UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Mobility { get; set; } = UserConsts.AtLeastXUniqueMusclesPerExercise_MobilityDefault;

    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityMin, UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityMax)]
    public int AtLeastXUniqueMusclesPerExercise_Flexibility { get; set; } = UserConsts.AtLeastXUniqueMusclesPerExercise_FlexibilityDefault;

    [Range(UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryMin, UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryMax)]
    public int AtLeastXUniqueMusclesPerExercise_Accessory { get; set; } = UserConsts.AtLeastXUniqueMusclesPerExercise_AccessoryDefault;

    [Range(UserConsts.FootnoteCountMin, UserConsts.FootnoteCountMax)]
    public int FootnoteCountTop { get; set; } = UserConsts.FootnoteCountTopDefault;

    [Range(UserConsts.FootnoteCountMin, UserConsts.FootnoteCountMax)]
    public int FootnoteCountBottom { get; set; } = UserConsts.FootnoteCountBottomDefault;

    [Range(UserConsts.WeightIsolationXTimesMoreMin, UserConsts.WeightIsolationXTimesMoreMax)]
    public double WeightIsolationXTimesMore { get; set; } = UserConsts.WeightIsolationXTimesMoreDefault;

    /// <summary>
    /// For muscle target volumes, weight secondary muscles less because they recover faster and don't create as strong of strengthening gains.
    /// 
    /// Don't use this for selecting a workout's exercises, those secondary muscles are valued as half of primary muscles.
    /// </summary>
    [Range(UserConsts.WeightSecondaryMusclesXTimesLessMin, UserConsts.WeightSecondaryMusclesXTimesLessMax)]
    public double WeightSecondaryMusclesXTimesLess { get; set; } = UserConsts.WeightSecondaryMusclesXTimesLessDefault;

    #endregion
    #region Navigation Properties

    [JsonInclude, InverseProperty(nameof(UserMuscleStrength.User))]
    public virtual ICollection<UserMuscleStrength> UserMuscleStrengths { get; init; } = [];

    [JsonInclude, InverseProperty(nameof(UserMuscleMobility.User))]
    public virtual ICollection<UserMuscleMobility> UserMuscleMobilities { get; init; } = [];

    [JsonInclude, InverseProperty(nameof(UserMuscleFlexibility.User))]
    public virtual ICollection<UserMuscleFlexibility> UserMuscleFlexibilities { get; init; } = [];

    [JsonInclude, InverseProperty(nameof(UserFrequency.User))]
    public virtual ICollection<UserFrequency> UserFrequencies { get; init; } = [];

    [JsonIgnore, InverseProperty(nameof(UserToken.User))]
    public virtual ICollection<UserToken> UserTokens { get; init; } = [];

    [JsonIgnore, InverseProperty(nameof(UserExercise.User))]
    public virtual ICollection<UserExercise> UserExercises { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserVariation.User))]
    public virtual ICollection<UserVariation> UserVariations { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserEmail.User))]
    public virtual ICollection<UserEmail> UserEmails { get; init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Footnote.UserFootnote.User))]
    public virtual ICollection<Footnote.UserFootnote> UserFootnotes { get; init; } = null!;

    //[JsonIgnore, InverseProperty(nameof(UserWorkout.User))]
    //public virtual ICollection<UserWorkout> UserWorkouts { get; init; } = null!;

    #endregion

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is User other
        && other.Id == Id;
}
