using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Equipment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Web.Controllers.Index;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserEditViewModel
{
    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserEditViewModel() { }

    public UserEditViewModel(Data.Entities.User.User user, string token)
    {
        User = user;
        Email = user.Email;
        SendDays = user.SendDays;
        IntensityLevel = user.IntensityLevel;
        Frequency = user.Frequency;
        Disabled = user.Disabled;
        DisabledReason = user.DisabledReason;
        EmailVerbosity = user.EmailVerbosity;
        PrehabFocus = user.PrehabFocus;
        RehabFocus = user.RehabFocus;
        FootnoteType = user.FootnoteType;
        MobilityMuscles = user.MobilityMuscles;
        ShowStaticImages = user.ShowStaticImages;
        SendHour = user.SendHour;
        DeloadAfterEveryXWeeks = user.DeloadAfterEveryXWeeks;
        RefreshAccessoryEveryXWeeks = user.RefreshAccessoryEveryXWeeks;
        RefreshFunctionalEveryXWeeks = user.RefreshFunctionalEveryXWeeks;
        IsNewToFitness = user.IsNewToFitness;
        SportsFocus = user.SportsFocus;
        SendMobilityWorkouts = user.SendMobilityWorkouts;
        Token = token;
    }

    public IList<Lib.ViewModels.Newsletter.ExerciseViewModel> TheIgnoredExercises { get; set; } = new List<Lib.ViewModels.Newsletter.ExerciseViewModel>();
    public IList<Lib.ViewModels.Newsletter.ExerciseViewModel> TheIgnoredVariations { get; set; } = new List<Lib.ViewModels.Newsletter.ExerciseViewModel>();

    public IList<UserEditFrequencyViewModel> UserFrequencies { get; set; } = new List<UserEditFrequencyViewModel>();

    [ValidateNever]
    public Data.Entities.User.User User { get; set; } = null!;

    /// <summary>
    /// If null, user has not yet tried to update.
    /// If true, user has successfully updated.
    /// If false, user failed to update.
    /// </summary>
    public bool? WasUpdated { get; set; }

    [DataType(DataType.EmailAddress)]
    [Required, RegularExpression(UserCreateViewModel.EmailRegex, ErrorMessage = UserCreateViewModel.EmailRegexError)]
    [Remote(nameof(IndexController.IsUserAvailable), IndexController.Name)]
    [Display(Name = "Email", Description = "We respect your privacy and sanity.")]
    public string Email { get; init; } = null!;

    public string Token { get; init; } = null!;

    [Required]
    [Display(Name = "I'm new to fitness", Description = "Simplifies workouts to just the core movements.")]
    public bool IsNewToFitness { get; init; }

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(UserConsts.DeloadAfterEveryXWeeksMin, UserConsts.DeloadAfterEveryXWeeksMax)]
    [Display(Name = "Deload After Every X Weeks", Description = "After how many weeks of strength training do you want to take a deload week?")]
    public int DeloadAfterEveryXWeeks { get; init; }

    [Required, Range(UserConsts.RefreshAccessoryEveryXWeeksMin, UserConsts.RefreshAccessoryEveryXWeeksMax)]
    [Display(Name = "Refresh Accessory Exercises Every X Weeks", Description = "How often should accessory exercises refresh?")]
    public int RefreshAccessoryEveryXWeeks { get; init; }

    [Required, Range(UserConsts.RefreshFunctionalEveryXWeeksMin, UserConsts.RefreshFunctionalEveryXWeeksMax)]
    [Display(Name = "Refresh Functional Exercises Every X Weeks", Description = "How often should exercises working functional movement patterns refresh?")]
    public int RefreshFunctionalEveryXWeeks { get; init; }

    [Required]
    [Display(Name = "Send Rest-Day Mobility Workouts", Description = "Will send emails on your rest days with daily mobility, stretching, prehab, and rehab exercises.")]
    public bool SendMobilityWorkouts { get; init; }

    /// <summary>
    /// Include a section to boost a specific sports performance
    /// </summary>
    [Display(Name = "Sports Focus (beta)", Description = "Include additional plyometric and strengthening exercises that focus on the movements involved in a particular sport.")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Prehab Focus (beta)", Description = "Focus areas to stretch and strengthen for injury prevention. Includes balance training.")]
    public PrehabFocus PrehabFocus { get; private set; }

    [Display(Name = "Mobility Muscles", Description = "Muscles targeted in the warmup and cooldown sections. These will be intersected with the current split's muscle groups.")]
    public MuscleGroups MobilityMuscles { get; private set; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises
    /// </summary>
    [Display(Name = "Rehab Focus (beta)")]
    public RehabFocus RehabFocus { get; init; }

    /// <summary>
    /// Types of footnotes to show to the user.
    /// </summary>
    [Display(Name = "Footnote Types", Description = "What types of footnotes do you want to see?")]
    public FootnoteType FootnoteType { get; set; }

    [Display(Name = "Disabled Reason")]
    public string? DisabledReason { get; init; }

    [Display(Name = "Disabled", Description = "Stop receiving email without deleting your account.")]
    public bool Disabled { get; init; }

    [Required]
    [Display(Name = "Workout Intensity", Description = "A beginner lifter should not immediately train heavy. Tendons lag behind muscles by 2-5 years in strength adaption. Don’t push harder or increase your loads at a rate faster than what your tendons can adapt to.")]
    public IntensityLevel IntensityLevel { get; init; }

    [Required]
    [Display(Name = "Workout Split", Description = "All splits will work the core muscles each day.")]
    public Frequency Frequency { get; init; }

    [Required]
    [Display(Name = "Email Verbosity", Description = "Choose the level of detail you want to receive in each email.")]
    public Verbosity EmailVerbosity { get; init; }

    [Required, Range(0, 23)]
    [Display(Name = "Send Time (UTC)", Description = "What hour of the day (UTC) do you want to receive emails?")]
    public int SendHour { get; init; }

    [Required]
    [Display(Name = "Show Static Images", Description = "Will show static images instead of animated images in the newsletter.")]
    public bool ShowStaticImages { get; set; }

    [Required]
    [Display(Name = "Send Days", Description = "Choose which days you want to receive the newsletter.")]
    public Days SendDays { get; private set; }

    [Display(Name = "Equipment", Description = "Choose equipment you have access to each day.")]
    public IList<Equipment> Equipment { get; set; } = new List<Equipment>();

    public int[]? EquipmentBinder { get; set; }

    [Display(Name = "Ignored Exercises", Description = "Choose exercises you want to ignore.")]
    public IList<Data.Entities.Exercise.Exercise> IgnoredExercises { get; set; } = new List<Data.Entities.Exercise.Exercise>();

    [Display(Name = "Ignored Variations", Description = "Choose variations you want to ignore.")]
    public IList<Data.Entities.Exercise.Variation> IgnoredVariations { get; set; } = new List<Data.Entities.Exercise.Variation>();

    public int[]? IgnoredExerciseBinder { get; set; }

    public int[]? IgnoredVariationBinder { get; set; }

    public PrehabFocus[]? PrehabFocusBinder
    {
        get => Enum.GetValues<PrehabFocus>().Where(e => PrehabFocus.HasFlag(e)).ToArray();
        set => PrehabFocus = value?.Aggregate(PrehabFocus.None, (a, e) => a | e) ?? PrehabFocus.None;
    }

    public MuscleGroups[]? MobilityMusclesBinder
    {
        get => Enum.GetValues<MuscleGroups>().Where(e => MobilityMuscles.HasFlag(e)).ToArray();
        set => MobilityMuscles = value?.Aggregate(MuscleGroups.None, (a, e) => a | e) ?? MuscleGroups.None;
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
}
