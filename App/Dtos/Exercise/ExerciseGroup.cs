using System.ComponentModel.DataAnnotations;

namespace App.Dtos.Exercise;

/// <summary>
/// Similar groups of exercises.
/// 
/// sa. Hand Planks and Forearm Planks should be in the same Planks group.
/// </summary>
[Flags]
public enum ExerciseGroup
{
    None = 0,

    [Display(Name = "Planks")]
    Planks = 1 << 0, // 1

    [Display(Name = "Handstands")]
    Handstands = 1 << 1, // 2

    /// <summary>
    /// Don't want too many grip exercises in a single workout
    /// </summary>
    [Display(Name = "Grip Strength")]
    GripStrength = 1 << 2, // 4
}
