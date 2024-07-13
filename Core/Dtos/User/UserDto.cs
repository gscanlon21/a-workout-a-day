﻿using Core.Consts;
using Core.Dtos.Footnote;
using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[Table("user")]
[DebuggerDisplay("Email = {Email}, LastActive = {LastActive}")]
public class UserDto
{
    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserDto() { }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public UserDto(string email, bool acceptedTerms, bool isNewToFitness)
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
    public bool ShowStaticImages { get; set; }

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
    public int WorkoutsDays => BitOperations.PopCount((ulong)SendDays);

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

    public virtual ICollection<UserMuscleStrengthDto> UserMuscleStrengths { get; init; } = [];

    public virtual ICollection<UserMuscleMobilityDto> UserMuscleMobilities { get; init; } = [];

    public virtual ICollection<UserMuscleFlexibilityDto> UserMuscleFlexibilities { get; init; } = [];

    public virtual ICollection<UserFrequencyDto> UserFrequencies { get; init; } = [];

    //[JsonIgnore]
    //public virtual ICollection<UserEmailDto> UserEmails { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<FootnoteDto> UserFootnotes { get; init; } = null!;

    #endregion
}
