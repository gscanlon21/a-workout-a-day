using FinerFettle.Web.Entities.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter;

public class ProficiencyViewModel
{
    public ProficiencyViewModel(Intensity intensity)
    {
        Intensity = intensity;
    }

    public Intensity Intensity { get; }

    public bool ShowName { get; init; } = false;
    public bool FirstTimeViewing { get; init; } = false;
}
