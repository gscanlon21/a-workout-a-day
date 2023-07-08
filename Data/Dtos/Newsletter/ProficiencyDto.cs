using Data.Entities.Exercise;
using Data.Entities.User;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyDto
{
    public ProficiencyDto(Intensity intensity, UserVariation? userVariation)
    {
        Intensity = intensity;
        UserVariation = userVariation;
    }

    public Intensity Intensity { get; }

    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
