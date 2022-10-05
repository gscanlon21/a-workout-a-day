using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ProficiencyViewModel
    {
        public ProficiencyViewModel(Intensity intensity)
        {
            Proficiency = intensity.Proficiency;
            Intensity = intensity;
        }

        public ProficiencyViewModel(IntensityPreference intensityPreference)
        {
            Proficiency = intensityPreference.Proficiency;
            Intensity = intensityPreference.Intensity;
            StrengtheningPreference = intensityPreference.StrengtheningPreference;
        }

        public Proficiency Proficiency { get; init; }
        public Intensity Intensity { get; init; }
        public StrengtheningPreference? StrengtheningPreference { get; init; }
    }
}
