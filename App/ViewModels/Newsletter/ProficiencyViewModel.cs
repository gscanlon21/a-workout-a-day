using App.Dtos.Exercise;
using App.Dtos.User;

namespace App.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyViewModel
{
    public ProficiencyViewModel(Intensity intensity, UserVariation? userVariation)
    {
        Intensity = intensity;
        UserVariation = userVariation;
    }

    public Intensity Intensity { get; }
    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
