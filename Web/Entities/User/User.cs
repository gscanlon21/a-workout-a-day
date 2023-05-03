using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Numerics;
using Web.Code.Extensions;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.Entities.User;

/// <summary>
/// User who signed up for the newsletter.
/// </summary>
[Table("user"), Comment("User who signed up for the newsletter")]
[Index(nameof(Email), IsUnique = true)]
[DebuggerDisplay("Email = {Email}, Disabled = {Disabled}")]
public class User
{
    #region Consts

    public const int DeloadAfterEveryXWeeksMin = 2;
    public const int DeloadAfterEveryXWeeksDefault = 10;
    public const int DeloadAfterEveryXWeeksMax = 18;

    public const int RefreshFunctionalEveryXWeeksMin = 0;
    public const int RefreshFunctionalEveryXWeeksDefault = 4;
    public const int RefreshFunctionalEveryXWeeksMax = 12;

    public const int RefreshAccessoryEveryXWeeksMin = 0;
    public const int RefreshAccessoryEveryXWeeksDefault = 1;
    public const int RefreshAccessoryEveryXWeeksMax = 12;

    #endregion

    private User()
    {
        EmailAtUTCOffset = 0;
        SendDays = RestDays.All;
        DeloadAfterEveryXWeeks = DeloadAfterEveryXWeeksDefault;
        RefreshAccessoryEveryXWeeks = RefreshAccessoryEveryXWeeksDefault;
        RefreshFunctionalEveryXWeeks = RefreshFunctionalEveryXWeeksDefault;
        EmailVerbosity = Verbosity.Normal;
        Frequency = Frequency.UpperLowerBodySplit4Day;
        StrengtheningPreference = StrengtheningPreference.Light;
        CreatedDate = DateOnly.FromDateTime(DateTime.UtcNow);
    }

    /// <summary>
    /// Creates a new user.
    /// </summary>
    public User(string email, bool acceptedTerms, bool isNewToFitness) : this()
    {
        Email = email.Trim();
        AcceptedTerms = acceptedTerms;
        IsNewToFitness = isNewToFitness;
        // User is new to fitness? Don't show the 'Adjunct' section so they don't feel overwhelmed
        IncludeAdjunct = !isNewToFitness;
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
    /// Include the Adjunct section in the newsletter.
    /// </summary>
    [Required]
    public bool IncludeAdjunct { get; set; }

    /// <summary>
    /// User prefers static instead of dynamic images?
    /// </summary>
    [Required]
    public bool PreferStaticImages { get; set; }

    /// <summary>
    /// User is new to fitness?
    /// </summary>
    [Required]
    public bool IsNewToFitness { get; set; }

    /// <summary>
    /// What hour of the day should we send emails to this user.
    /// </summary>
    [Required, Range(0, 23)]
    public int EmailAtUTCOffset { get; set; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises.
    /// </summary>
    [Required]
    public MuscleGroups RecoveryMuscle { get; set; }

    /// <summary>
    /// Include a section to boost a specific sports performance.
    /// </summary>
    [Required]
    public SportsFocus SportsFocus { get; set; }

    /// <summary>
    /// Days the user want to skip the newsletter.
    /// </summary>
    [NotMapped]
    public RestDays RestDays => RestDays.All.UnsetFlag32(SendDays);

    /// <summary>
    /// Days the user want to skip the newsletter.
    /// </summary>
    [Required]
    public RestDays SendDays { get; set; }

    /// <summary>
    /// Whan this user was created.
    /// </summary>
    [Required]
    public DateOnly CreatedDate { get; private init; }

    /// <summary>
    /// How intense the user wants workouts to be.
    /// </summary>
    [Required]
    public StrengtheningPreference StrengtheningPreference { get; set; }

    /// <summary>
    /// The user's preferred workout split.
    /// </summary>
    [Required]
    public Frequency Frequency { get; set; }

    /// <summary>
    /// How often should we show a deload week to the user?
    /// </summary>
    [Required, Range(DeloadAfterEveryXWeeksMin, DeloadAfterEveryXWeeksMax)]
    public int DeloadAfterEveryXWeeks { get; set; }

    /// <summary>
    /// How often to refresh functional movement exercises.
    /// </summary>
    [Required, Range(RefreshFunctionalEveryXWeeksMin, RefreshFunctionalEveryXWeeksMax)]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    /// <summary>
    /// How often to refresh accessory exercises.
    /// </summary>
    [Required, Range(RefreshAccessoryEveryXWeeksMin, RefreshAccessoryEveryXWeeksMax)]
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

    [NotMapped]
    public bool IsDemoUser => Features.HasFlag(Features.Demo);

    [NotMapped]
    public bool Disabled => DisabledReason != null;

    /// <summary>
    /// How many days of the week is the user working out?
    /// </summary>
    [NotMapped]
    public int WorkoutsDays => BitOperations.PopCount((ulong)SendDays);

    [NotMapped]
    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId) ?? new List<int>();

    #endregion


    #region Navigation Properties

    [InverseProperty(nameof(UserEquipment.User))]
    public virtual ICollection<UserEquipment> UserEquipments { get; private init; } = new List<UserEquipment>();

    [InverseProperty(nameof(UserToken.User))]
    public virtual ICollection<UserToken> UserTokens { get; private init; } = new List<UserToken>();

    [InverseProperty(nameof(UserExercise.User))]
    public virtual ICollection<UserExercise> UserExercises { get; private init; } = null!;

    [InverseProperty(nameof(UserVariation.User))]
    public virtual ICollection<UserVariation> UserVariations { get; private init; } = null!;

    [InverseProperty(nameof(UserExerciseVariation.User))]
    public virtual ICollection<UserExerciseVariation> UserExerciseVariations { get; private init; } = null!;

    [InverseProperty(nameof(Newsletter.Newsletter.User))]
    public virtual ICollection<Newsletter.Newsletter> Newsletters { get; private init; } = null!;

    #endregion
}
