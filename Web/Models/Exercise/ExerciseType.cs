using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Main/SportsRecovery.
/// </summary>
[Flags]
public enum ExerciseType
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Strength")]
    Strength = 1 << 0, // 1

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Power")]
    Power = 1 << 1, // 2

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Endurance")]
    Endurance = 1 << 2, // 4

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Flexibility")]
    Flexibility = 1 << 3, // 8

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Stability")]
    Stability = 1 << 4, // 16

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Mobility")]
    Mobility = Flexibility | Stability, // 24

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Agility")]
    Agility = 1 << 5, // 32 

    All = Strength | Power | Endurance | Flexibility | Stability | Mobility | Agility
}
