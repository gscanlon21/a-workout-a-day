
using Data.Entities.Exercise;
using Data.Entities.User;
using System.Text.Json.Serialization;

namespace Api.ViewModels.Newsletter;

/// <summary>
/// Viewmodel for Proficiency.cshtml
/// </summary>
public class ProficiencyViewModel
{
    public ProficiencyViewModel(Intensity intensity, User.UserNewsletterViewModel? user, UserVariation? userVariation, bool demo)
    {
        Intensity = intensity;
        UserVariation = userVariation;
        User = user;
        Demo = demo;
    }

    public bool Demo { get; }
    public Intensity Intensity { get; }

    [JsonIgnore]
    public User.UserNewsletterViewModel? User { get; }

    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
