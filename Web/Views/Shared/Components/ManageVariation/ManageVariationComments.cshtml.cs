using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Web.Views.User;

namespace Web.Views.Shared.Components.ManageVariation;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageVariationCommentsViewModel
{
    [ValidateNever]
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    [ValidateNever]
    public required Data.Entities.Users.User User { get; init; }

    public required List<string> VariationComments { get; init; }
}
