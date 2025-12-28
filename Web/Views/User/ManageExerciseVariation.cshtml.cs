using Core.Dtos.Newsletter;
using Core.Models.Newsletter;

namespace Web.Views.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageExerciseVariationViewModel
{
    public record Params(Section Section, string Email, string Token, int ExerciseId, int VariationId);

    public bool? WasUpdated { get; init; }
    public required Params Parameters { get; init; }
    public required Data.Entities.Users.User User { get; init; }
    public required ExerciseVariationDto ExerciseVariation { get; init; } = null!;

    public Verbosity VariationVerbosity => Verbosity.Instructions;
}
