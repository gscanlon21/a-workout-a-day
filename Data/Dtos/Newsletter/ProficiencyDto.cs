using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Dtos.User;
using System.Text.Json.Serialization;

namespace Data.Dtos.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyDto
{
    public ProficiencyDto(Intensity intensity, UserNewsletterDto? user, UserVariation? userVariation, bool demo)
    {
        Intensity = intensity;
        UserVariation = userVariation;
        User = user;
        Demo = demo;
    }

    public bool Demo { get; }
    public Intensity Intensity { get; }

    [JsonIgnore]
    public UserNewsletterDto? User { get; }

    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
