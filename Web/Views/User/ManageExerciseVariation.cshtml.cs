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

    /// <summary>
    /// True if being managed from a variation context.
    /// False if being managed from an exercise context.
    /// </summary>
    public required bool HasVariation { get; init; }

    public Verbosity VariationVerbosity => Verbosity.Instructions;
}
