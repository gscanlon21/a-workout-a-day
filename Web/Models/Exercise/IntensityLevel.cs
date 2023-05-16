using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Endurance/Hypertrophy/Strength/Stabilization/Recovery/Warmup/Cooldown
/// </summary>
public enum IntensityLevel
{
    /// <summary>
    /// For Isotonic/Isokinetic exercises, 2 x 12-15 rep range.
    /// For Isometric/Plyometric exercises, ~4x30s.
    /// </summary>
    [Display(Name = "Light", Description = "The target range for muscle failure will consist of few sets of many reps—ideal for lifting lighter weights and building muscle endurance.")]
    Light = 0,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 3 x 8-12 rep range.
    /// For Isometric/Plyometric exercises, ~3x40s.
    /// </summary>
    [Display(Name = "Medium", Description = "The target range for muscle failure will consist of a medial number of sets and reps—ideal for lifting medium weights and building muscle mass.")]
    Medium = 1,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 4 x 6-8 rep range.
    /// For Isometric/Plyometric exercises, ~2x60s.
    /// </summary>
    [Display(Name = "Heavy", Description = "The target range for muscle failure will consist of many sets of few reps—ideal for lifting heavy weights and building muscle strength.")]
    Heavy = 2,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 1 x 15-20 rep range.
    /// For Isometric/Plyometric exercises, ~5x24s.
    /// </summary>
    [Display(Name = "Endurance")]
    Endurance = 3,

    /// <summary>
    /// Used for recovery tracks
    /// </summary>
    [Display(Name = "Recovery")]
    Recovery = 4,

    /// <summary>
    /// Used for warmups
    /// </summary>
    [Display(Name = "Warmup")]
    Warmup = 5,

    /// <summary>
    /// Used for cooldowns
    /// </summary>
    [Display(Name = "Cooldown")]
    Cooldown = 6,
}
