using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attribute
    public class ExerciseViewModel
    {
        public static bool IsIsometric(ExerciseViewModel viewModel)
        {
            return viewModel.Variation.MuscleContractions.HasFlag(MuscleContractions.Isometric);
        }

        public static bool IsWeighted(ExerciseViewModel viewModel)
        {
            return viewModel.Variation.EquipmentGroups.Any(eg => eg.IsWeight);
        }

        public bool IsIntensityLevel(IntensityLevel intensityLevel)
        {
            return Proficiencies.Any(p => p.Intensity.IntensityLevel == intensityLevel);
        }

        public ExerciseViewModel(Models.User.User? user, Variation variation, IntensityLevel? intensityLevel, ExerciseActivityLevel activityLevel)
        {
            User = user;
            Exercise = variation.Exercise;
            Variation = variation;
            IntensityLevel = intensityLevel ?? (IntensityLevel?)user?.StrengtheningPreference;
            ActivityLevel = activityLevel;

            if (user != null)
            {
                Verbosity = user.EmailVerbosity;
            }
        }

        /// <summary>
        /// Is this exercise a warmup/cooldown or main exercise?
        /// </summary>
        public ExerciseActivityLevel ActivityLevel { get; }

        public Models.User.User? User { get; }

        public Exercise Exercise { get; }
        public Variation Variation { get; }

        public IntensityLevel? IntensityLevel { get; }

        [UIHint("Proficiency")]
        public IList<ProficiencyViewModel> Proficiencies => Variation.Intensities
            .Where(intensity => intensity.IntensityLevel == IntensityLevel || IntensityLevel == null)
            .Select(intensity => new ProficiencyViewModel(intensity) { ShowName = IntensityLevel == null }).ToList();

        public Models.User.UserExercise? UserExercise { get; set; }

        public bool HasLowerProgressionVariation { get; set; }
        public bool HasHigherProgressionVariation { get; set; }

        /// <summary>
        /// How much detail to show of the exercise?
        /// </summary>
        public Verbosity Verbosity { get; init; } = Verbosity.Normal;

        /// <summary>
        /// Should hide detail not shown in the landing page demo?
        /// </summary>
        public bool Demo { get; init; } = false;
    }
}
