using Core.Dtos.Newsletter;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.User;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Web.Views.Index;

namespace Web.Views.User;


/// <summary>
/// For CRUD actions
/// </summary>
public class UserEditViewModel
{
    [Obsolete("Public parameterless constructor required for model binding.", error: true)]
    public UserEditViewModel() { }

    public UserEditViewModel(Data.Entities.User.User user, string token)
    {
        User = user;
        Token = token;
        Email = user.Email;
        SendHour = user.SendHour;
        SendDays = user.SendDays;
        ImageType = user.ImageType;
        Intensity = user.Intensity;
        Frequency = user.Frequency;
        Verbosity = user.Verbosity;
        Equipment = user.Equipment;
        RehabFocus = user.RehabFocus;
        RehabSkills = user.RehabSkills;
        PrehabFocus = user.PrehabFocus;
        SportsFocus = user.SportsFocus;
        SportsSkills = user.SportsSkills;
        FootnoteType = user.FootnoteType;
        IsNewToFitness = user.IsNewToFitness;
        SecondSendHour = user.SecondSendHour;
        DeloadAfterXWeeks = user.DeloadAfterXWeeks;
        NewsletterEnabled = user.NewsletterEnabled;
        IncludeMobilityWorkouts = user.IncludeMobilityWorkouts;
        NewsletterDisabledReason = user.NewsletterDisabledReason;
        UserFrequencies = user.UserFrequencies.OrderBy(uf => uf.Id)
            .Select(uf => new UserEditFrequencyViewModel(uf)).ToList();
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

    [Display(Name = "Prehab Skills")]
    public IList<UserEditPrehabSkillViewModel> UserPrehabSkills { get; set; } = [];

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

    [Display(Name = "Sports Skills", Description = "Skills to focus on during sports exercises.")]
    public int SportsSkills { get; set; }

    [Display(Name = "Prehab Focus", Description = "Focus areas to stretch and strengthen for injury prevention. Includes balance training.")]
    public PrehabFocus PrehabFocus { get; private set; }

    /// <summary>
    /// Don't strengthen this muscle group, but do show recovery variations for exercises
    /// </summary>
    [Display(Name = "Rehab Focus", Description = "Focuses on body mechanics and muscle activation for injured muscles.")]
    public RehabFocus RehabFocus { get; init; }

    [Display(Name = "Rehab Skills", Description = "Skills to focus on during rehabilitation.")]
    public int RehabSkills { get; set; }

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

    [Range(UserConsts.SendHourMin, UserConsts.SendHourMax)]
    [Display(Name = "Second Send Time (UTC)", Description = "What hour of the day (UTC) do you want to receive a second mobility workout?")]
    public int? SecondSendHour { get; init; }

    [Required]
    [Display(Name = "Image Type", Description = "How should images appear in your workouts?")]
    public ImageType ImageType { get; set; }

    [Required]
    [Display(Name = "Strengthening Days", Description = "What days do you want to receive new strengthening workouts?")]
    public Days SendDays { get; private set; }

    [Display(Name = "Equipment", Description = "What equipment do you have access to?")]
    public Equipment Equipment { get; set; }

    public int[]? RehabSkillsBinder
    {
        get => RehabFocus.GetSkillType()?.AllValues.Select(Convert.ToInt32).Where(e => (RehabSkills & e) == e).ToArray();
        set => RehabSkills = value?.Aggregate(0, (a, e) => a | e) ?? 0;
    }

    public int[]? SportsSkillsBinder
    {
        get => SportsFocus.GetSkillType()?.AllValues.Select(Convert.ToInt32).Where(e => (SportsSkills & e) == e).ToArray();
        set => SportsSkills = value?.Aggregate(0, (a, e) => a | e) ?? 0;
    }

    public Verbosity[]? VerbosityBinder
    {
        get => Enum.GetValues<Verbosity>().Where(e => Verbosity.HasFlag(e)).ToArray();
        set => Verbosity = value?.Aggregate(Verbosity.None, (a, e) => a | e) ?? Verbosity.None;
    }

    public PrehabFocus[]? PrehabFocusBinder
    {
        get => EnumExtensions.GetValuesExcluding(PrehabFocus.None, PrehabFocus.All).Where(e => PrehabFocus.HasFlag(e)).ToArray();
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

    public class UserEditPrehabSkillViewModel
    {
        //[Obsolete("Public parameterless constructor required for model binding.", error: false)]
        public UserEditPrehabSkillViewModel() { }

        public UserEditPrehabSkillViewModel(UserPrehabSkill userMuscleMobility)
        {
            Count = userMuscleMobility.Count;
            UserId = userMuscleMobility.UserId;
            Skills = userMuscleMobility.Skills;
            PrehabFocus = userMuscleMobility.PrehabFocus;
            AllRefreshed = userMuscleMobility.AllRefreshed;
        }

        public PrehabFocus PrehabFocus { get; init; }

        public int UserId { get; init; }

        [Range(UserConsts.PrehabCountMin, UserConsts.PrehabCountMax)]
        [Display(Name = "Count", Description = "The max number of exercises to choose.")]
        public int Count { get; set; }

        [Display(Name = "Only Refreshed Exercises?", Description = "Skip exercises with refresh padding.")]
        public bool AllRefreshed { get; set; }

        [Display(Name = "Skills", Description = "What skills to focus on?")]
        public int Skills { get; set; }

        public int[]? PrehabSkillsBinder
        {
            get => PrehabFocus.GetSkillType()?.AllValues.Select(Convert.ToInt32).Where(e => (Skills & e) == e).ToArray();
            set => Skills = value?.Aggregate(0, (a, e) => a | e) ?? 0;
        }
    }

    public class UserEditMuscleMobilityViewModel
    {
        public UserEditMuscleMobilityViewModel() { }

        public UserEditMuscleMobilityViewModel(UserMuscleMobility userMuscleMobility)
        {
            UserId = userMuscleMobility.UserId;
            MuscleGroup = userMuscleMobility.MuscleGroup;
            Count = userMuscleMobility.Count;
        }

        public MusculoskeletalSystem MuscleGroup { get; init; }

        public int UserId { get; init; }

        [Range(UserConsts.UserMuscleMobilityMin, UserConsts.UserMuscleMobilityMax)]
        public int Count { get; set; }
    }

    public class UserEditMuscleFlexibilityViewModel
    {
        public UserEditMuscleFlexibilityViewModel() { }

        public UserEditMuscleFlexibilityViewModel(UserMuscleFlexibility userMuscleMobility)
        {
            UserId = userMuscleMobility.UserId;
            MuscleGroup = userMuscleMobility.MuscleGroup;
            Count = userMuscleMobility.Count;
        }

        public MusculoskeletalSystem MuscleGroup { get; init; }

        public int UserId { get; init; }

        [Range(UserConsts.UserMuscleFlexibilityMin, UserConsts.UserMuscleFlexibilityMax)]
        public int Count { get; set; }
    }

    public class UserEditFrequencyViewModel : IValidatableObject
    {
        public UserEditFrequencyViewModel()
        {
            Hide = true;
        }

        public UserEditFrequencyViewModel(WorkoutRotationDto rotation)
        {
            Hide = false;
            Day = rotation.Id;
            MuscleGroups = rotation.MuscleGroups;
            MovementPatterns = rotation.MovementPatterns;
        }

        public UserEditFrequencyViewModel(UserFrequency frequency)
        {
            Hide = false;
            Day = frequency.Rotation.Id;
            MuscleGroups = frequency.Rotation.MuscleGroups;
            MovementPatterns = frequency.Rotation.MovementPatterns;
        }

        public bool Hide { get; set; }

        public int Day { get; init; }

        public MovementPattern MovementPatterns { get; set; }

        public IList<MusculoskeletalSystem>? MuscleGroups { get; set; }

        public MovementPattern[]? MovementPatternsBinder
        {
            get => Enum.GetValues<MovementPattern>().Where(e => MovementPatterns.HasFlag(e)).ToArray();
            set => MovementPatterns = value?.Aggregate(MovementPattern.None, (a, e) => a | e) ?? MovementPattern.None;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Hide)
            {
                if (MovementPatterns == MovementPattern.None && MuscleGroups?.Any() != true)
                {
                    yield return new ValidationResult("At least one movement pattern or muscle group is required.", [nameof(Day)]);
                }
            }
        }
    }
}

