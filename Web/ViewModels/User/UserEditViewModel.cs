using Core.Consts;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserEditViewModel
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserEditViewModel() { }

    public UserEditViewModel(Data.Entities.User.User user, string token)
    {
        User = user;
        Email = user.Email;
        SendHour = user.SendHour;
        SendDays = user.SendDays;
        Intensity = user.Intensity;
        Frequency = user.Frequency;
        Verbosity = user.Verbosity;
        Equipment = user.Equipment;
        RehabFocus = user.RehabFocus;
        PrehabFocus = user.PrehabFocus;
        SportsFocus = user.SportsFocus;
        FootnoteType = user.FootnoteType;
        IsNewToFitness = user.IsNewToFitness;
        ShowStaticImages = user.ShowStaticImages;
        DeloadAfterXWeeks = user.DeloadAfterXWeeks;
        NewsletterEnabled = user.NewsletterEnabled;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
        NewsletterDisabledReason = user.NewsletterDisabledReason;
        
        Token = token;
    }

    [ValidateNever]
    public Data.Entities.User.User User { get; set; } = null!;

    public string Token { get; set; } = null!;

    /// <summary>
    /// If null, user has not yet tried to update.
    /// If true, user has successfully updated.
    /// If false, user failed to update.
    /// </summary>
    public bool? WasUpdated { get; set; }

    public IList<UserEditFrequencyViewModel> UserFrequencies { get; set; } = [];

    [Display(Name = "Mobility Muscle Targets", Description = "Customize muscle targets for the warmup section. These will be intersected with the current split's muscle groups.")]
    public IList<UserEditMuscleMobilityViewModel> UserMuscleMobilities { get; set; } = [];

    [Display(Name = "Flexibility Muscle Targets", Description = "Customize muscle targets for the cooldown section.")]
    public IList<UserEditMuscleFlexibilityViewModel> UserMuscleFlexibilities { get; set; } = [];

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(UserCreateViewModel.EmailRegex, ErrorMessage = UserCreateViewModel.EmailRegexError)]
    [Display(Name = "Email", Description = "")]
    public string Email { get; init; } = null!;

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplify your workouts.")]
    public bool IsNewToFitness { get; init; }

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(UserConsts.DeloadAfterXWeeksMin, UserConsts.DeloadAfterXWeeksMax)]
    [Display(Name = "Deload After Every X Weeks", Description = "After how many weeks of strength training do you want to take a deload week?")]
    public int DeloadAfterXWeeks { get; init; }

    [Required]
    [Display(Name = "Include Rest-Day Mobility Workouts", Description = "Include workouts on your rest days with core, mobility, flexibility, injury prevention, and rehabilitation exercises.")]
    public bool IncludeMobilityWorkouts { get; init; }

    /// <summary>
    /// Include a section to boost a specific sports performance
    /// </summary>
    [Display(Name = "Sports Focus", Description = "Include additional plyometric and strengthening exercises that focus on the movements involved in a particular sport.")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Prehab Focus", Description = "Focus areas to stretch and strengthen for injury prevention. Includes balance training.")]
    public PrehabFocus PrehabFocus { get; private set; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises
    /// </summary>
    [Display(Name = "Rehab Focus", Description = "Focuses on body mechanics and muscle activation for injured muscles.")]
    public RehabFocus RehabFocus { get; init; }

    /// <summary>
    /// Types of footnotes to show to the user.
    /// </summary>
    [Display(Name = "Footnotes", Description = "What types of footnotes do you want to see?")]
    public FootnoteType FootnoteType { get; set; }

    [Display(Name = "Disabled Reason")]
    public string? NewsletterDisabledReason { get; init; }

    [Display(Name = "Subscribe to Workout Emails", Description = "Receive your workouts via email.")]
    public bool NewsletterEnabled { get; init; }

    [Required]
    [Display(Name = "Workout Intensity", Description = "Beginner lifters should not immediately train heavy. Tendons lag behind muscles by 2-5 years in strength adaption. Don’t push harder or increase your loads at a rate faster than what your tendons can adapt to.")]
    public Intensity Intensity { get; init; }

    [Required]
    [Display(Name = "Workout Split", Description = "")]
    public Frequency Frequency { get; init; }

    [Required]
    [Display(Name = "Workout Verbosity", Description = "What level of detail do you want to receive in each workout?")]
    public Verbosity Verbosity { get; set; }

    [Required, Range(UserConsts.SendHourMin, UserConsts.SendHourMax)]
    [Display(Name = "Send Time (UTC)", Description = "What hour of the day (UTC) do you want to receive new workouts?")]
    public int SendHour { get; init; }

    [Required]
    [Display(Name = "Show Static Images", Description = "Show static images instead of animated images in the workouts.")]
    public bool ShowStaticImages { get; set; }

    [Required]
    [Display(Name = "Strengthening Days", Description = "What days do you want to receive new strengthening workouts?")]
    public Days SendDays { get; private set; }

    [Display(Name = "Equipment", Description = "What equipment do you have access to workout with?")]
    public Equipment Equipment { get; set; }

    public Verbosity[]? VerbosityBinder
    {
        get => Enum.GetValues<Verbosity>().Where(e => Verbosity.HasFlag(e)).ToArray();
        set => Verbosity = value?.Aggregate(Verbosity.None, (a, e) => a | e) ?? Verbosity.None;
    }

    public PrehabFocus[]? PrehabFocusBinder
    {
        get => Enum.GetValues<PrehabFocus>().Where(e => PrehabFocus.HasFlag(e)).ToArray();
        set => PrehabFocus = value?.Aggregate(PrehabFocus.None, (a, e) => a | e) ?? PrehabFocus.None;
    }

    public FootnoteType[]? FootnoteTypeBinder
    {
        get => Enum.GetValues<FootnoteType>().Where(e => FootnoteType.HasFlag(e)).ToArray();
        set => FootnoteType = value?.Aggregate(FootnoteType.None, (a, e) => a | e) ?? FootnoteType.None;
    }

    public Days[]? SendDaysBinder
    {
        get => Enum.GetValues<Days>().Where(e => SendDays.HasFlag(e)).ToArray();
        set => SendDays = value?.Aggregate(Days.None, (a, e) => a | e) ?? Days.None;
    }

    public Equipment[]? EquipmentBinder
    {
        get => Enum.GetValues<Equipment>().Where(e => Equipment.HasFlag(e)).ToArray();
        set => Equipment = value?.Aggregate(Equipment.None, (a, e) => a | e) ?? Equipment.None;
    }
}
