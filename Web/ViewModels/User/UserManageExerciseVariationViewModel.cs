namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageExerciseVariationViewModel
{
    public required UserManageExerciseViewModel Exercise { get; set; }
    public required UserManageVariationViewModel? Variation { get; set; }

    public bool? WasUpdated { get; init; }
}
