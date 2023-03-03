using Web.Entities.Exercise;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for IgnoreVariation.cshtml
/// </summary>
public class IgnoreVariationViewModel
{
    public Entities.Exercise.Exercise? Exercise { get; init; }

    public required Variation Variation { get; init; }

    public bool? WasUpdated { get; init; }
}
