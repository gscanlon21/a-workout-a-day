using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Newsletter;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[Table("user"), Comment("User who signed up for the newsletter")]
[Index(nameof(Email), IsUnique = true)]
[DebuggerDisplay("Email = {Email}, LastActive = {LastActive}")]
public class User
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
        Verbosity = UserConsts.VerbosityDefault;
        Frequency = UserConsts.FrequencyDefault;
        FootnoteType = UserConsts.FootnotesDefault;
        Intensity = UserConsts.IntensityDefault;
        DeloadAfterEveryXWeeks = UserConsts.DeloadAfterEveryXWeeksDefault;
        RefreshAccessoryEveryXWeeks = UserConsts.RefreshAccessoryEveryXWeeksDefault;
        RefreshFunctionalEveryXWeeks = UserConsts.RefreshFunctionalEveryXWeeksDefault;

        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// The user's email address.
    /// </summary>
    [Required]
    public string Email { get; private init; } = null!;

    /// <summary>
    /// User has accepted the current Terms of Use when they signed up.
    /// </summary>
    [Required]
    public bool AcceptedTerms { get; private init; }

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    [Required]
    public bool ShowStaticImages { get; set; }

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
    /// Whan this user was created.
    /// </summary>
    [Required]
    public DateOnly CreatedDate { get; private init; }

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

    /// <summary>
    /// Don't use in queries, is not mapped currently.
    /// </summary>
    [NotMapped]
    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId) ?? new List<int>();

    #endregion


    #region Navigation Properties

    [JsonIgnore, InverseProperty(nameof(UserEquipment.User))]
    public virtual ICollection<UserEquipment> UserEquipments { get; private init; } = new List<UserEquipment>();

    [JsonIgnore, InverseProperty(nameof(UserMuscleStrength.User))]
    public virtual ICollection<UserMuscleStrength> UserMuscleStrengths { get; private init; } = new List<UserMuscleStrength>();

    [JsonIgnore, InverseProperty(nameof(UserMuscleMobility.User))]
    public virtual ICollection<UserMuscleMobility> UserMuscleMobilities { get; private init; } = new List<UserMuscleMobility>();

    [JsonIgnore, InverseProperty(nameof(UserMuscleFlexibility.User))]
    public virtual ICollection<UserMuscleFlexibility> UserMuscleFlexibilities { get; private init; } = new List<UserMuscleFlexibility>();

    [JsonIgnore, InverseProperty(nameof(UserFrequency.User))]
    public virtual ICollection<UserFrequency> UserFrequencies { get; private init; } = new List<UserFrequency>();

    [JsonIgnore, InverseProperty(nameof(UserToken.User))]
    public virtual ICollection<UserToken> UserTokens { get; private init; } = new List<UserToken>();

    [JsonIgnore, InverseProperty(nameof(UserVariationWeight.User))]
    public virtual ICollection<UserVariationWeight> UserVariationWeights { get; private init; } = new List<UserVariationWeight>();

    [JsonIgnore, InverseProperty(nameof(UserExercise.User))]
    public virtual ICollection<UserExercise> UserExercises { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserVariation.User))]
    public virtual ICollection<UserVariation> UserVariations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserExerciseVariation.User))]
    public virtual ICollection<UserExerciseVariation> UserExerciseVariations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserWorkout.User))]
    public virtual ICollection<UserWorkout> UserWorkouts { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserEmail.User))]
    public virtual ICollection<UserEmail> UserEmails { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Footnote.Footnote.User))]
    public virtual ICollection<Footnote.Footnote> UserFootnotes { get; private init; } = null!;

    #endregion
}
