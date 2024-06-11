using Core.Consts;
using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Lib.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Web.ViewModels.User;

namespace Web.ViewModels.Components.UserVariation;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageVariationViewModel
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [ValidateNever]
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    [ValidateNever]
    public required Data.Entities.User.User User { get; init; }

    [ValidateNever]
    [Display(Name = "Refreshes After", Description = "Refresh this variation—the next workout will try and select a new exercise variation if available.")]
    public required Data.Entities.User.UserVariation UserVariation { get; init; }

    [ValidateNever]
    [Display(Name = "Variation", Description = "Ignore this variation for just this section.")]
    public required Variation Variation { get; init; }

    [ValidateNever]
    public required IList<ExerciseVariationViewModel> Variations { get; init; } = null!;

    public Verbosity VariationVerbosity => Verbosity.Instructions | Verbosity.Images;

    [Required, Range(0, 999)]
    [Display(Name = "How much weight are you able to lift?")]
    public int Weight { get; init; }

    [Required, Range(0, 6)]
    [Display(Name = "How many sets did you do?")]
    public int Sets { get; init; }

    [Required, Range(0, 60)]
    [Display(Name = "How many reps/secs did you do?")]
    public int Reps { get; init; }

    [Required, Range(UserConsts.RefreshEveryXWeeksMin, UserConsts.RefreshEveryXWeeksMax)]
    [Display(Name = "Refresh Every X Weeks", Description = "How often do you want to refresh this variation?")]
    public int RefreshEveryXWeeks { get; init; }
}
