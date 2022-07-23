using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Variation exercise, IntensityLevel desiredIntensity)
        {
            Exercise = exercise;
            Intensity = exercise.Intensities.Single(i => i.IntensityLevel == desiredIntensity);
        }

        public MuscleGroups Muscles { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public Variation Exercise { get; init; }
        public Intensity Intensity { get; init; }
    }
}
