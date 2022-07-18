namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum MuscleContractions
    {
        /// <summary>
        /// The muscle contracts and stays the same size. Holds.
        /// </summary>
        Isometric = 1 << 0,

        /// <summary>
        /// The muscle contracts and shortens. Pulling motion. 
        /// </summary>
        Concentric = 1 << 1,

        /// <summary>
        /// The muscle contracts and lengthens. Pushing motion.
        /// </summary>
        Eccentric = 1 << 2 
    }
}
