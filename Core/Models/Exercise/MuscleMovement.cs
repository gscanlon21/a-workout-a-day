using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric.
/// </summary>
[Flags]
public enum MuscleMovement
{
    /// <summary>
    /// In isometric exercises, the muscles do not shorten or lengthen. 
    /// These exercises are often referred to as static. 
    /// For example, a person may hold a plank position and therefore no muscles shorten or lengthen. 
    /// Other isometric exercise examples are abdominal vacuums and bridges.
    /// </summary>
    [Display(Name = "Isometric")]
    Isometric = 1 << 0, // 1

    /// <summary>
    /// With isotonic exercises, the same amount of muscle tension or weight is maintained throughout the movement. 
    /// An example is performing a bicep curl as the weight remains the same as the joint moves through the entire range of motion. 
    /// Other examples of isotonic exercises include push-ups, pull-ups, and squats. 
    /// The weight of the body remains the same as the joint moves between the start and end ranges.
    /// </summary>
    [Display(Name = "Isotonic")]
    Isotonic = 1 << 1, // 2

    /// <summary>
    /// Plyometrics, also known as jump training or plyos, are exercises in which muscles exert maximum force in short intervals of time, with the goal of increasing power (speed-strength). 
    /// This training focuses on learning to move from a muscle extension to a contraction in a rapid or "explosive" manner, such as in specialized repeated jumping.
    /// </summary>
    [Display(Name = "Plyometric")]
    Plyometric = 1 << 2, // 4

    /// <summary>
    /// Isokinetic exercise involves motion, but the speed stays the same, such as with a stationary bike. 
    /// The word isokinetic contains kinetic, which means motion. 
    /// With a stationary bike, the leg moves through the circular motion of the pedal circumferences, but the speed of limb motion and revolutions per minute stays the same. 
    /// In isokinetic exercises, the resistance can vary, such as using the resistance adjustment in an exercise bike. 
    /// Often specialized equipment is used to promote isokinetic exercise. Another example is a treadmill. This allows target speeds and resistance to be adjusted and dialed in for the individual. Even allows specific muscle groups to be targeted. 
    /// Overall, isokinetic exercises can help to improve muscular strength and endurance.
    /// </summary>
    [Display(Name = "Isokinetic")]
    Isokinetic = 1 << 3, // 8

    All = Isometric | Isotonic | Plyometric | Isokinetic
}
