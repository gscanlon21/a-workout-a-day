using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Similar groups of exercises. So we don't see two similar exercises in a workout.
/// 
/// sa. Hand Planks and Forearm Planks should be in the same Planks group.
/// </summary>
[Flags]
public enum ExerciseGroup
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Horizontal core isometrics.
    /// </summary>
    [Display(Name = "Planks")]
    Planks = 1 << 0, // 1

    /// <summary>
    /// Half-vertical half-horizontal core isometrics.
    /// </summary>
    [Display(Name = "Pikes")]
    Pikes = 1 << 1, // 2

    /// <summary>
    /// Vertical core isometrics.
    /// </summary>
    [Display(Name = "Handstands")]
    Handstands = 1 << 2, // 4

    /// <summary>
    /// Don't want too many pushup exercises in a single workout.
    /// </summary>
    [Display(Name = "Pushups")]
    Pushups = 1 << 3, // 8

    /// <summary>
    /// Don't want too many grip exercises in a single workout.
    /// </summary>
    [Display(Name = "Grip Strength")]
    GripStrength = 1 << 4, // 16

    All = Planks | Pikes | Handstands | Pushups | GripStrength
}
