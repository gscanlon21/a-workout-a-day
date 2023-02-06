using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric.
/// </summary>
[Flags]
public enum MuscleMovement
{
    /// <summary>
    /// Isometric exercises involve constant muscle contraction without changing the actual length of your muscles.
    /// </summary>
    [Display(Name = "Isometric")] 
    Isometric = 1 << 0, // 1

    /// <summary>
    /// Isotonic exercises require a constant load or resistance while moving your muscles. Independent of the speed you do the exercise, you just need to focus on the resistance itself. 
    /// </summary>
    [Display(Name = "Isotonic")]
    Isotonic = 1 << 1, // 2

    /// <summary>
    /// Plyometric exercises are all about exerting the maximum force in a short time, also called explosive training
    /// </summary>
    [Display(Name = "Plyometric")]
    Plyometric = 1 << 2, // 4

    /// <summary>
    /// Isokinetic exercises are pretty much the opposite of Isotonic exercises. The resistance can change during the exercises, but you do them over a set period of time or a set number of exercises.
    /// </summary>
    [Display(Name = "Isokinetic")]
    Isokinetic = 1 << 3, // 8
}
