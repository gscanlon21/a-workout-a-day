namespace FinerFettle.Web.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric.
/// </summary>
public enum MuscleMovement
{
    /// <summary>
    /// Isometric exercises involve constant muscle contraction without changing the actual length of your muscles.
    /// </summary>
    Isometric = 1,

    /// <summary>
    /// Isotonic exercises require a constant load or resistance while moving your muscles. Independent of the speed you do the exercise, you just need to focus on the resistance itself. 
    /// </summary>
    Isotonic = 2,

    /// <summary>
    /// Isokinetic exercises are pretty much the opposite of Isotonic exercises. The resistance can change during the exercises, but you do them over a set period of time or a set number of exercises.
    /// </summary>
    Isokinetic = 3,

    /// <summary>
    /// Plyometric exercises are all about exerting the maximum force in a short time, also called explosive training
    /// </summary>
    Pylometric = 4,
}
