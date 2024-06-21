using Core.Models.Newsletter;

namespace Web.Views.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class ManageExerciseVariationDto
{
    public record Params(Section Section, string Email, string Token, int ExerciseId, int VariationId);

    public required Params Parameters { get; init; }
    public required Data.Entities.User.User User { get; init; }
    public required bool HasVariation { get; init; }

    public bool? WasUpdated { get; init; }
}
