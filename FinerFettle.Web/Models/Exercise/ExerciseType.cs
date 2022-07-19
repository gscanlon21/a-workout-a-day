namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum ExerciseType
    {
        /// <summary>
        /// Rest
        /// </summary>
        None = 0,

        /// <summary>
        /// Cardio
        /// </summary>
        Aerobic = 1 << 0,

        /// <summary>
        /// Weight or resistance training
        /// </summary>
        Strength = 1 << 1,

        /// <summary>
        /// Muscle control
        /// </summary>
        Stability = 1 << 2,

        /// <summary>
        /// Muscle range of motion and movement
        /// </summary>
        Flexibility = 1 << 3,

        /// <summary>
        /// Warm-up or cool-down exercises
        /// </summary>
        Stretch = 1 << 4
    }

    public class ExerciseTypeGroups
    {
        public const ExerciseType StretchStrength = ExerciseType.Stretch | ExerciseType.Strength;
        public const ExerciseType StretchAerobic = ExerciseType.Stretch | ExerciseType.Aerobic;
        public const ExerciseType StabilityFlexibility = ExerciseType.Stability | ExerciseType.Flexibility;
    }
}
