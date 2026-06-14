using Data.Entities.Exercises;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using Web.Views.User;

namespace Web.Views.Shared.Components.UpsertVariation;

/// <summary>
/// For CRUD actions
/// </summary>
public class UpsertVariationViewModel
{
    [ValidateNever]
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    [ValidateNever]
    public required Data.Entities.Users.User User { get; init; }

    [ValidateNever]
    [Display(Name = "Variation", Description = "")]
    public required Variation Variation { get; init; } = null!;
}
