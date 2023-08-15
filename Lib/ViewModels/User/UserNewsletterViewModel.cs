using Core.Consts;
using Core.Models.Exercise;
using Core.Models.Footnote;
using Core.Models.User;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

/// <summary>
/// For the newsletter
/// </summary>
public class UserNewsletterViewModel
{
    [Display(Name = "Days Until Deload")]
    public TimeSpan TimeUntilDeload { get; set; } = TimeSpan.Zero;

    public int Id { get; init; }

    public string Email { get; init; } = null!;

    public string Token { get; init; } = null!;

    public Features Features { get; init; }

    [Display(Name = "Footnotes")]
    public FootnoteType FootnoteType { get; init; }

    public bool ShowStaticImages { get; init; }

    public bool IncludeMobilityWorkouts { get; init; }

    public DateOnly? LastActive { get; init; }

    [Display(Name = "Is New to Fitness")]
    public bool IsNewToFitness { get; init; }

    [Display(Name = "Strengthening Days")]
    public Days SendDays { get; init; }

    [Display(Name = "Prehab Focus")]
    public PrehabFocus PrehabFocus { get; init; }

    [Display(Name = "Rehab Focus")]
    public RehabFocus RehabFocus { get; init; }

    [Display(Name = "Sports Focus")]
    public SportsFocus SportsFocus { get; init; }

    [Display(Name = "Workout Intensity")]
    public Intensity Intensity { get; init; }

    [Display(Name = "Workout Split")]
    public Frequency Frequency { get; init; }

    [Display(Name = "Weeks Between Functional Refresh")]
    public int RefreshFunctionalEveryXWeeks { get; set; }

    [Display(Name = "Weeks Between Accessory Refresh")]
    public int RefreshAccessoryEveryXWeeks { get; set; }

    [JsonInclude]
    public ICollection<UserExerciseViewModel> UserExercises { get; init; } = null!;

    [JsonInclude]
    public ICollection<UserVariationViewModel> UserVariations { get; init; } = null!;

    [JsonInclude]
    public ICollection<UserEquipmentViewModel> UserEquipments { get; init; } = null!;

    public IEnumerable<int> EquipmentIds => UserEquipments.Select(e => e.EquipmentId);

    public bool IsAlmostInactive => LastActive.HasValue && LastActive.Value < DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(-1 * (UserConsts.DisableAfterXMonths - 1));
}
