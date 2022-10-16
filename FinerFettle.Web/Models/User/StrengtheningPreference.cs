namespace FinerFettle.Web.Models.User
{
    /// <summary>
    /// Frequency of strengthing days
    /// </summary>
    public enum StrengtheningPreference
    {
        /// <summary>
        /// Strength exercises are always full-body exercises.
        /// </summary>
        Maintain = 0,

        /// <summary>
        /// Strength exercises rotate between upper body, mid body, and lower body.
        /// </summary>
        Obtain = 1,

        /// <summary>
        /// Strength exercises alternate between upper body and mid/lower body.
        /// </summary>
        Gain = 2,

        /// <summary>
        /// Endurance exercises work the full-body every day..
        /// </summary>
        Endurance = 3
    }
}
