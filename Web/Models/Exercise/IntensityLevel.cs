using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Endurance/Hypertrophy/Strength/Stabilization/Recovery/Warmup/Cooldown
/// </summary>
public enum IntensityLevel
{
    /// <summary>
    /// For Isotonic/Isokinetic exercises, 12-15 rep range.
    /// For Isometric/Plyometric exercises, ~4x30s.
    /// </summary>
    [Display(Name = "Endurance")]
    Endurance = 0,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 8-12 rep range.
    /// For Isometric/Plyometric exercises, ~3x40s.
    /// </summary>
    [Display(Name = "Hypertrophy")]
    Hypertrophy = 1,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 6-8 rep range.
    /// For Isometric/Plyometric exercises, ~2x60s.
    /// </summary>
    [Display(Name = "Strength")]
    Strength = 2,

    /// <summary>
    /// For Isotonic/Isokinetic exercises, 15-20 rep range.
    /// For Isometric/Plyometric exercises, ~5x24s.
    /// </summary>
    [Display(Name = "Stabilization")]
    Stabilization = 3,

    [Display(Name = "Recovery")]
    Recovery = 4,

    [Display(Name = "Warmup")]
    Warmup = 5,

    [Display(Name = "Cooldown")]
    Cooldown = 6,
}
