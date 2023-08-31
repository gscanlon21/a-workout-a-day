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
    /// Forearm planks, hand planks, side planks.
    /// </summary>
    [Display(Name = "Planks")]
    Planks = 1 << 0, // 1

    /// <summary>
    /// Pikes.
    /// </summary>
    [Display(Name = "Pikes")]
    Pikes = 1 << 1, // 2

    /// <summary>
    /// Handstands.
    /// </summary>
    [Display(Name = "Handstands")]
    Handstands = 1 << 2, // 4

    /// <summary>
    /// Pushups, pike pushups, handstand pushups.
    /// </summary>
    [Display(Name = "Pushups")]
    Pushups = 1 << 3, // 8

    /// <summary>
    /// Don't want too many grip exercises in a single workout.
    /// </summary>
    [Display(Name = "Grip Strength")]
    GripStrength = 1 << 4, // 16

    /// <summary>
    /// Bent-knee calf raises, straight-leg calf raises.
    /// </summary>
    [Display(Name = "Calf Raises")]
    CalfRaises = 1 << 5, // 32

    /// <summary>
    /// Bodyweight squats, front squats, rack squats, overhead squats, suitcase squats.
    /// </summary>
    [Display(Name = "Squats")]
    Squats = 1 << 6, // 64

    All = Planks | Pikes | Handstands | Pushups | GripStrength | CalfRaises | Squats
}
