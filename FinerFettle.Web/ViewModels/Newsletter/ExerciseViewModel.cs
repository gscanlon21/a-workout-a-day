using FinerFettle.Web.Models.Exercise;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    public class ExerciseViewModel
    {
        public MuscleGroups Muscles { get; set; }
        public ExerciseType ExerciseType { get; set; }
        public Variation Exercise { get; set; }
        public Intensity Intensity { get; set; }
    }
}
