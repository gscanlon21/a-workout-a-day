using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ProficiencyViewModel
    {
        public ProficiencyViewModel(Intensity intensity)
        {
            Intensity = intensity;
        }

        public Intensity Intensity { get; init; }
        public bool ShowName { get; set; } = false;
    }
}
