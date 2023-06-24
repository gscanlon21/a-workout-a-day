using Lib.ViewModels.Exercise;
using Lib.ViewModels.User;

namespace Lib.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyViewModel
{
    public ProficiencyViewModel(IntensityViewModel intensity, UserVariationViewModel? userVariation)
    {
        Intensity = intensity;
        UserVariation = userVariation;
    }

    public IntensityViewModel Intensity { get; }
    public UserVariationViewModel? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
