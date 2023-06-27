using Core.Models.Newsletter;
using Data.Entities.Equipment;
using Data.Models.User;
using System.ComponentModel.DataAnnotations;

namespace Data.Models.Newsletter;

/// <summary>
/// Viewmodel for Debug.cshtml
/// </summary>
public class DebugModel
{
    /// <summary>
    /// The number of footnotes to show in the newsletter
    /// </summary>
    public readonly int FootnoteCount = 2;

    public DebugModel(Entities.User.User user, string token)
    {
        User = new UserNewsletterModel(user, token);
        Verbosity = user.Verbosity;
    }

    public UserNewsletterModel User { get; }

    /// <summary>
    /// How much detail to show in the newsletter.
    /// </summary>
    public Verbosity Verbosity { get; private init; }

    public required IList<ExerciseModel> DebugExercises { get; init; }

    /// <summary>
    /// Display which equipment the user does not have.
    /// </summary>
    public EquipmentModel Equipment { get; init; } = null!;
}
