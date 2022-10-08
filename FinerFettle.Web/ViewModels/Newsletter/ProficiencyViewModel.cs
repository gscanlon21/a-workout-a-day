using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ProficiencyViewModel
    {
        public ProficiencyViewModel(Intensity intensity, StrengtheningPreference? preference)
        {
            Intensity = intensity;
            StrengtheningPreference = preference;

            if (preference == null)
            {
                Proficiencies = intensity.IntensityPreferences
                                    .NullIfEmpty()?.Select(ip => Tuple.Create(ip.StrengtheningPreference.GetSingleDisplayName(), ip.Proficiency))
                                    .ToList() ?? new List<Tuple<string, Proficiency>>() { Tuple.Create(StrengtheningPreference?.GetSingleDisplayName() ?? "Default", intensity.Proficiency) };
            }
            else
            {
                Proficiencies = intensity.IntensityPreferences
                                    .Where(ip => ip.StrengtheningPreference == preference)
                                    .NullIfEmpty()?.Select(ip => Tuple.Create(ip.StrengtheningPreference.GetSingleDisplayName(), ip.Proficiency))
                                    .ToList() ?? new List<Tuple<string, Proficiency>>() { Tuple.Create(StrengtheningPreference?.GetSingleDisplayName() ?? "Default", intensity.Proficiency) };
            }
        }

        public IList<Tuple<string, Proficiency>> Proficiencies { get; init; }
        public Intensity Intensity { get; init; }
        public StrengtheningPreference? StrengtheningPreference { get; init; }
    }
}
