using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Main/SportsRecovery.
/// </summary>
[Flags]
public enum ExerciseType
{
    /// <summary>
    /// Rest
    /// </summary>
    //None = 0,

    /// <summary>
    /// Weight or resistance training. 
    /// Anerobic.
    /// </summary>
    [Display(Name = "Main")]
    Main = 1 << 0, // 1

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Sports")]
    Sports = 1 << 1, // 2

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Rehab")]
    Rehab = 1 << 2, // 4

    All = Main | Sports | Rehab
}
