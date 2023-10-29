using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Isometric/Concentric/Eccentric.
/// </summary>
[Flags]
public enum MuscleMovement
{
    /// <summary>
    /// These are strength exercises where your muscles contract while you hold a still position.
    /// </summary>
    [Display(Name = "Isometric")]
    Isometric = 1 << 0, // 1

    /// <summary>
    /// Isotonic exercises are those that put a consistent tension on the muscles while moving them through a full range of motion.
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
    /// Isokinetic exercise involves performing movements at a constant speed with varying resistance.
    /// </summary>
    [Display(Name = "Isokinetic")]
    Isokinetic = 1 << 3, // 8

    All = Isometric | Isotonic | Plyometric | Isokinetic
}
