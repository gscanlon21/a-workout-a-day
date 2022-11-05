using FinerFettle.Web.Attributes.Data;
using FinerFettle.Web.Entities.Equipment;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserViewModel
{
    public UserViewModel() { }

    public UserViewModel(Entities.User.User user, string token) 
    {
        Email = user.Email;
        AcceptedTerms = user.AcceptedTerms;
        RestDays = user.RestDays;
        StrengtheningPreference = user.StrengtheningPreference;
        Disabled = user.Disabled;
        DisabledReason = user.DisabledReason;
        EmailVerbosity = user.EmailVerbosity;
        PrefersWeights = user.PrefersWeights;
        RecoveryMuscle = user.RecoveryMuscle;
        IncludeBonus = user.IncludeBonus;
        SportsFocus = user.SportsFocus;
        Token = token;
    }

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

    [Required, RegularExpression(@"\s*\S+@\S+\.\S+\s*", ErrorMessage = "Invalid email.")]
    [Remote(nameof(Controllers.UserValidationController.IsUserAvailable), Controllers.UserValidationController.Name, ErrorMessage = "Invalid email. Manage your preferences from the previous newsletter.")]
    [DisplayName("Email"), DataType(DataType.EmailAddress)]
    public string Email { get; init; } = null!;

    public string? Token { get; init; }

    [Required, MustBeTrue]
    public bool AcceptedTerms { get; init; }

    /// <summary>
    /// Anti-bot honeypot
    /// </summary>
    [Required, MustBeTrue(DisableClientSideValidation = true, ErrorMessage = "Bot, I am your father.")]
    public bool IExist { get; init; }

    /// <summary>
    /// Pick weighted variations over calisthenics if available
    /// </summary>
    [Required, DisplayName("Prefer Weights")]
    public bool PrefersWeights { get; init; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises
    /// </summary>
    [DisplayName("Recovery Muscle (beta)")]
    public MuscleGroups RecoveryMuscle { get; init; }

    /// <summary>
    /// Include a section to boost a specific sports performance
    /// </summary>
    [DisplayName("Sports Focus (beta)")]
    public SportsFocus SportsFocus { get; init; }

    [DisplayName("Disabled Reason")]
    public string? DisabledReason { get; init; }

    [DisplayName("Disabled")]
    public bool Disabled { get; init; }

    [DisplayName("Include Bonus Exercises")]
    public bool IncludeBonus { get; init; }

    [Required]
    [DisplayName("Strengthening Preference")]
    public StrengtheningPreference StrengtheningPreference { get; init; }

    [Required]
    [DisplayName("Email Verbosity")]
    public Verbosity EmailVerbosity { get; init; }

    [Required]
    [DisplayName("Rest Days")]
    public RestDays RestDays { get; private set; }

    [DisplayName("Equipment")]
    public IList<Equipment> Equipment { get; set; } = new List<Equipment>();

    public int[]? EquipmentBinder { get; init; }

    [DisplayName("Ignored Exercises")]
    public IList<Entities.Exercise.Exercise> IgnoredExercises { get; init; } = new List<Entities.Exercise.Exercise>();

    public int[]? IgnoredExerciseBinder { get; init; }

    public RestDays[]? RestDaysBinder
    {
        get => Enum.GetValues<RestDays>().Cast<RestDays>().Where(e => RestDays.HasFlag(e)).ToArray();
        set => RestDays = value?.Aggregate(RestDays.None, (a, e) => a | e) ?? RestDays.None;
    }
}
