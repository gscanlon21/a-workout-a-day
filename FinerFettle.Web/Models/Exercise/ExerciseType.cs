namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Cardio/Strength/Stability/Flexibility.
    /// </summary>
    [Flags]
    public enum ExerciseType
    {
        /// <summary>
        /// Rest
        /// </summary>
        None = 0,

        /// <summary>
        /// Cardio. 
        /// Aerobic.
        /// </summary>
        Cardio = 1 << 0, // 1

        /// <summary>
        /// Weight or resistance training. 
        /// Anerobic.
        /// </summary>
        Strength = 1 << 1, // 2

        /// <summary>
        /// Muscle control
        /// </summary>
        Stability = 1 << 2, // 4

        /// <summary>
        /// Muscle range of motion and movement. Most stretches are included in this.
        /// </summary>
        Flexibility = 1 << 3, // 8
    }
}
