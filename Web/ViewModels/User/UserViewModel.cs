using Web.Attributes.Data;
using Web.Entities.Equipment;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserViewModel
{
    public UserViewModel() { }

    public UserViewModel(Entities.User.User user, string token) 
    {
        User = user;
        Email = user.Email;
        AcceptedTerms = user.AcceptedTerms;
        RestDays = user.RestDays;
        StrengtheningPreference = user.StrengtheningPreference;
        Frequency = user.Frequency;
        Disabled = user.Disabled;
        DisabledReason = user.DisabledReason;
        EmailVerbosity = user.EmailVerbosity;
        PrefersWeights = user.PrefersWeights;
        RecoveryMuscle = user.RecoveryMuscle;
        DeloadAfterEveryXWeeks = user.DeloadAfterEveryXWeeks;
        IncludeBonus = user.IncludeBonus;
        IsNewToFitness = user.IsNewToFitness;
        SportsFocus = user.SportsFocus;
        Token = token;
    }

    public Entities.User.User? User { get; set; }

    /// <summary>
    /// If null, user has not yet tried to subscribe.
    /// If true, user has successfully subscribed.
    /// If false, user failed to subscribe.
    /// </summary>
    public bool? WasSubscribed { get; set; }

    /// <summary>
    /// If null, user has not yet tried to unsubscribe.
    /// If true, user has successfully unsubscribed.
    /// If false, user failed to unsubscribe.
    /// </summary>
    public bool? WasUnsubscribed { get; set; }

    /// <summary>
    /// If null, user has not yet tried to update.
    /// If true, user has successfully updated.
    /// If false, user failed to update.
    /// </summary>
    public bool? WasUpdated { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(@"\s*\S+@\S+\.\S+\s*", ErrorMessage = "Invalid email.")]
    [Remote(nameof(Controllers.UserValidationController.IsUserAvailable), Controllers.UserValidationController.Name, ErrorMessage = "Invalid email. Manage your preferences from the previous newsletter.")]
    [Display(Name = "Email", Description = "We respect your privacy and sanity.")]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    [Required, MustBeTrue]
    [Display(Description = "You must be at least 13 years old.")]
    public bool AcceptedTerms { get; init; }

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplifies the first few months of workouts to help make a habit out of working out.")]
    public bool IsNewToFitness { get; init; } = true;

    /// <summary>
    /// Anti-bot honeypot
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    [Display(Description = "If you're reading this, you're human.")]
    public bool IExist { get; init; }

    /// <summary>
    /// Pick weighted variations over calisthenics if available
    /// </summary>
    [Required]
    [Display(Name = "Prefer Weights", Description = "Prioritize exercises that use free weights over bodyweight variations.")]
    public bool PrefersWeights { get; init; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises
    /// </summary>
    [Display(Name = "Recovery Muscle (beta)")]
    public MuscleGroups RecoveryMuscle { get; init; }

    public const int DeloadAfterEveryXWeeksMin = 2;
    public const int DeloadAfterEveryXWeeksMax = 18;

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(DeloadAfterEveryXWeeksMin, DeloadAfterEveryXWeeksMax)]
    [Display(Name = "Deload After Every X Weeks")]
    public int DeloadAfterEveryXWeeks { get; set; } = 4;

    /// <summary>
    /// Include a section to boost a specific sports performance
    /// </summary>
    [Display(Name = "Sports Focus (beta)", Description = "Include additional exercises that focus on the muscles and movements involved in a particular sport.")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Disabled Reason")]
    public string? DisabledReason { get; init; }

    [Display(Name = "Disabled", Description = "Stop receiving email without deleting your account.")]
    public bool Disabled { get; init; }

    [Display(Name = "Include Bonus Exercises", Description = "Select this to add more exercise variety to your workout.")]
    public bool IncludeBonus { get; init; }

    [Required]
    [Display(Name = "Strengthening Preference")]
    public StrengtheningPreference StrengtheningPreference { get; init; }

    [Required]
    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    [Required]
    [Display(Name = "Email Verbosity", Description = "Choose the level of detail you want to receive in each email.")]
    public Verbosity EmailVerbosity { get; init; }

    [Required, Range(0, 23)]
    [Display(Name = "Send Emails at this Hour (UTC)", Description = "What hour of the day (UTC) do you want to receive emails?")]
    public int EmailAtUTCOffset { get; set; } = 0;

    [Required]
    [Display(Name = "Rest Days", Description = "Choose which days you want to take a break.")]
    public RestDays RestDays { get; private set; }

    [Display(Name = "Equipment", Description = "Choose equipment you have access to each day.")]
    public IList<Equipment> Equipment { get; set; } = new List<Equipment>();

    public int[]? EquipmentBinder { get; init; }

    [Display(Name = "Ignored Exercises", Description = "Choose exercises you want to ignore.")]
    public IList<Entities.Exercise.Exercise> IgnoredExercises { get; init; } = new List<Entities.Exercise.Exercise>();

    public int[]? IgnoredExerciseBinder { get; init; }

    public RestDays[]? RestDaysBinder
    {
        get => Enum.GetValues<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
        set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
    }
}
