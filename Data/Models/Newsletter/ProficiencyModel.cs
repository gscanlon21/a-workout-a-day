using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Models.User;
using System.Text.Json.Serialization;

namespace Data.Models.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyModel
{
    public ProficiencyModel(Intensity intensity, UserNewsletterModel? user, UserVariation? userVariation, bool demo)
    {
        Intensity = intensity;
        UserVariation = userVariation;
        User = user;
        Demo = demo;
    }

    public bool Demo { get; }
    public Intensity Intensity { get; }

    [JsonIgnore]
    public UserNewsletterModel? User { get; }

    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
