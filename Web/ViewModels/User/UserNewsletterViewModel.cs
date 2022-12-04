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
        IncludeBonus = user.IncludeBonus;
        IncludeAdjunct = user.IncludeAdjunct;
        IsNewToFitness = user.IsNewToFitness;
        UserExercises = user.UserExercises;
        SportsFocus = user.SportsFocus;
        LastActive = user.LastActive;
        EmailVerbosity = user.EmailVerbosity;
        Token = token;
    }

    public int Id { get; }

    public string Email { get; }

    public string Token { get; }

    [Display(Name = "Include Bonus Exercises")]
    public Bonus IncludeBonus { get; init; }

    [Display(Name = "Include Workout Adjunct")]
    public bool IncludeAdjunct { get; init; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; set; }

    [Display(Name = "Rest Days")]
    public RestDays RestDays { get; init; }

    [Display(Name = "Recovery Muscle")]
    public MuscleGroups RecoveryMuscle { get; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Email Verbosity")]
    public Verbosity EmailVerbosity { get; init; }

    [Display(Name = "Strengthening Preference")]
    public StrengtheningPreference StrengtheningPreference { get; init; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    public ICollection<UserExercise> UserExercises { get; init; }

    public ICollection<UserEquipment> UserEquipments { get; init; }

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public DateOnly? LastActive { get; init; } = null;

    public bool IsAlmostInactive => LastActive != null && LastActive < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-5);
}
