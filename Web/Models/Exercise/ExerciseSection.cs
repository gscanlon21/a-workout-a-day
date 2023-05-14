using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Main/WarmupCooldown.
/// </summary>
[Flags]
public enum ExerciseSection
{
    /// <summary>
    /// Rest
    /// </summary>
    //None = 0,

    /// <summary>
    /// Weight or resistance training. 
    /// Anerobic.
    /// </summary>
    [Display(Name = "Main")]
    Main = 1 << 0, // 1

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Warmup/Cooldown")]
    WarmupCooldown = 1 << 1, // 2

    All = Main | WarmupCooldown
}
