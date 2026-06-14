using Data.Entities.Exercises;
using System.ComponentModel.DataAnnotations;
using Web.Views.User;

namespace Web.Views.Shared.Components.UpsertExercise;

/// <summary>
/// For CRUD actions
/// </summary>
public class UpsertExerciseViewModel
{
    public required ManageExerciseVariationViewModel.Params Parameters { get; init; }

    public required Data.Entities.Users.User User { get; init; }

    [Display(Name = "Exercise", Description = "")]
    public required Exercise Exercise { get; init; }
}
