using Web.Entities.Exercise;
using Web.Entities.User;

namespace Web.ViewModels.Newsletter;

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
    public User.UserNewsletterViewModel? User { get; }
    public UserVariation? UserVariation { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
