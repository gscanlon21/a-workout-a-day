using System.ComponentModel.DataAnnotations;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.ViewModels.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterViewModel
{
    public UserNewsletterViewModel(Entities.User.User user, string token)
    {
        Id = user.Id;
        Email = user.Email;
        RecoveryMuscle = user.RecoveryMuscle;
        RestDays = user.RestDays;
        UserEquipments = user.UserEquipments;
        StrengtheningPreference = user.StrengtheningPreference;
        Frequency = user.Frequency;
        IncludeAdjunct = user.IncludeAdjunct;
        PreferStaticImages = user.PreferStaticImages;
        IsNewToFitness = user.IsNewToFitness;
        UserExercises = user.UserExercises;
        UserVariations = user.UserVariations;
        SportsFocus = user.SportsFocus;
        LastActive = user.LastActive;
        EmailVerbosity = user.EmailVerbosity;
        Token = token;
    }

    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    [Display(Name = "Days Until Functional Refresh")]
    public TimeSpan TimeUntilFunctionalRefresh { get; set; } = TimeSpan.Zero;

    [Display(Name = "Days Until Accessory Refresh")]
    public TimeSpan TimeUntilAccessoryRefresh { get; set; } = TimeSpan.Zero;

    public int Id { get; }

    public string Email { get; }

    public string Token { get; }

    public bool PreferStaticImages { get; }

    public DateOnly? LastActive { get; }

    [Display(Name = "Include Workout Adjunct")]
    public bool IncludeAdjunct { get; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; }

    [Display(Name = "Rest Days")]
    public RestDays RestDays { get; }

    [Display(Name = "Recovery Muscle")]
    public MuscleGroups RecoveryMuscle { get; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Email Verbosity")]
    public Verbosity EmailVerbosity { get; }

    [Display(Name = "Strengthening Level")]
    public StrengtheningPreference StrengtheningPreference { get; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; }

    public ICollection<UserExercise> UserExercises { get; init; }

    public ICollection<UserVariation> UserVariations { get; init; }

    public ICollection<UserEquipment> UserEquipments { get; init; }

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public bool IsAlmostInactive => LastActive != null && LastActive < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-5);
}
