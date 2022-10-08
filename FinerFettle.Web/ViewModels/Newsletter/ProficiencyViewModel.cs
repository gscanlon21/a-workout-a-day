using FinerFettle.Web.Extensions;
using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ProficiencyViewModel
    {
        public ProficiencyViewModel(Variation intensity, IntensityLevel? preference)
        {
            Intensity = intensity;
            IntensityLevel = preference;

            if (preference == null)
            {
                Proficiencies = intensity.IntensityPreferences
                                    .NullIfEmpty()?.Select(ip => Tuple.Create(ip.IntensityLevel.GetSingleDisplayName(), ip.Proficiency))
                                    .ToList() ?? new List<Tuple<string, Proficiency>>() { Tuple.Create(IntensityLevel?.GetSingleDisplayName() ?? "Default", intensity.Proficiency) };
            }
            else
            {
                Proficiencies = intensity.IntensityPreferences
                                    .Where(ip => ip.IntensityLevel == preference)
                                    .NullIfEmpty()?.Select(ip => Tuple.Create(ip.IntensityLevel.GetSingleDisplayName(), ip.Proficiency))
                                    .ToList() ?? new List<Tuple<string, Proficiency>>() { Tuple.Create(IntensityLevel?.GetSingleDisplayName() ?? "Default", intensity.Proficiency) };
            }
        }

        public IList<Tuple<string, Proficiency>> Proficiencies { get; init; }
        public Variation Intensity { get; init; }
        public IntensityLevel? IntensityLevel { get; init; }
    }
}
