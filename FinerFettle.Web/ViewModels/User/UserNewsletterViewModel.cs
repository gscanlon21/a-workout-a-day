using FinerFettle.Web.Entities.User;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;

namespace FinerFettle.Web.ViewModels.User;

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
        PrefersWeights = user.PrefersWeights;
        IncludeBonus = user.IncludeBonus;
        UserExercises = user.UserExercises;
        SportsFocus = user.SportsFocus;
        LastActive = user.LastActive;
        EmailVerbosity = user.EmailVerbosity;
        Token = token;
    }

    public int Id { get; }

    public string Email { get; }

    public string Token { get; }

    public bool PrefersWeights { get; init; }
    
    public bool IncludeBonus { get; init; }

    public RestDays RestDays { get; init; }

    public MuscleGroups RecoveryMuscle { get; }

    public SportsFocus SportsFocus { get; init; }

    public Verbosity EmailVerbosity { get; init; }

    public StrengtheningPreference StrengtheningPreference { get; init; }
    
    public Frequency Frequency { get; init; }

    public ICollection<UserExercise> UserExercises { get; init; }

    public ICollection<UserEquipment> UserEquipments { get; init; }

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public DateOnly? LastActive { get; init; } = null;

    public bool IsAlmostInactive => LastActive != null && LastActive < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-5);
}
