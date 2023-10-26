using Core.Models.Newsletter;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageExerciseVariationViewModel
{
    public record Parameters(Section Section, string Email, string Token, int ExerciseId, int VariationId);

    public required UserManageExerciseViewModel Exercise { get; set; }
    public required UserManageVariationViewModel? Variation { get; set; }

    public bool? WasUpdated { get; init; }
}
