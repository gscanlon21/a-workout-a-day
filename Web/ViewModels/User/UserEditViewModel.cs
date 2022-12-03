using Web.Attributes.Data;
using Web.Entities.Equipment;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserEditViewModel
{
    // Must have a public parameterless constructor for form posts
    public UserEditViewModel() { }

    public UserEditViewModel(Entities.User.User user, string token) 
    {
        User = user;
        Email = user.Email;
        RestDays = user.RestDays;
        StrengtheningPreference = user.StrengtheningPreference;
        Frequency = user.Frequency;
        Disabled = user.Disabled;
        DisabledReason = user.DisabledReason;
        EmailVerbosity = user.EmailVerbosity;
        PrefersWeights = user.PrefersWeights;
        RecoveryMuscle = user.RecoveryMuscle;
        IncludeAdjunct = user.IncludeAdjunct;
        EmailAtUTCOffset = user.EmailAtUTCOffset;
        DeloadAfterEveryXWeeks = user.DeloadAfterEveryXWeeks;
        IncludeBonus = user.IncludeBonus;
        IsNewToFitness = user.IsNewToFitness;
        SportsFocus = user.SportsFocus;
        Token = token;
    }

    public Entities.User.User? User { get; set; }

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

    public string Token { get; init; }

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplifies workouts to help make working out a habit.")]
    public bool IsNewToFitness { get; init; }

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

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(Entities.User.User.DeloadAfterEveryXWeeksMin, Entities.User.User.DeloadAfterEveryXWeeksMax)]
    [Display(Name = "Deload After Every X Weeks")]
    public int DeloadAfterEveryXWeeks { get; init; }

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

    [Display(Name = "Include Workout Adjunct", Description = "Select this to add more exercises to your workout.")]
    public bool IncludeAdjunct { get; init; }

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
    public int EmailAtUTCOffset { get; init; }

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
