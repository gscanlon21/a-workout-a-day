using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attributes
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Variation exercise, Intensity intensity, MuscleGroups muscles, ExerciseType type)
        {
            Exercise = exercise;
            Intensity = intensity;
            Muscles = muscles;
            ExerciseType = type;

            if (intensity.IntensityLevel == IntensityLevel.Main)
            {
                ActivityLevel = ExerciseActivityLevel.Main;
            } 
            else if (intensity.IntensityLevel == IntensityLevel.Stretch)
            {
                if (exercise.MuscleContractions.HasFlag(MuscleContractions.Isometric))
                {
                    // Choose static stretches for cooldown exercises
                    ActivityLevel = ExerciseActivityLevel.Cooldown;
                } 
                else
                {
                    // Choose dynamic stretches for warmup exercises
                    ActivityLevel = ExerciseActivityLevel.Warmup;
                }
            }
        }

        public ExerciseActivityLevel ActivityLevel { get; }

        public MuscleGroups Muscles { get; init; }

        public ExerciseType ExerciseType { get; init; }

        public Variation Exercise { get; init; }

        [UIHint(nameof(Intensity))]
        public Intensity Intensity { get; init; }

        public IList<Equipment> Equipment => Exercise.EquipmentGroups.SelectMany(e => e.Equipment).ToList();
    }
}
