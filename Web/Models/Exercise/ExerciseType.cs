using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

[Flags]
public enum NewsletterType
{
    /// <summary>
    /// Rest
    /// </summary>
    None = 0,

    /// <summary>
    /// Cardio. 
    /// Aerobic.
    /// </summary>
    //[Display(Name = "Cardio")]
    //Cardio = 1 << 0, // 1

    /// <summary>
    /// Weight or resistance training. 
    /// Anerobic.
    /// </summary>
    [Display(Name = "Strength")]
    Strength = 1 << 1, // 2

    /// <summary>
    /// Muscle control
    /// </summary>
    [Display(Name = "Stability")]
    Stability = 1 << 2, // 4

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    //[Display(Name = "Warmup/Cooldown")]
    //WarmupCooldown = 1 << 3, // 8

    /// <summary>
    /// Primary mover of strength.
    /// </summary>
    //[Display(Name = "Super Strength")]
    //SuperStrength = 1 << 4, // 16
}


/// <summary>
/// Cardio/Strength/Stability/Flexibility.
/// </summary>
[Flags]
public enum ExerciseType
{
    /// <summary>
    /// Rest
    /// </summary>
    None = 0,

    /// <summary>
    /// Cardio. 
    /// Aerobic.
    /// </summary>
    //[Display(Name = "Cardio")]
    //Cardio = 1 << 0, // 1

    /// <summary>
    /// Weight or resistance training. 
    /// Anerobic.
    /// </summary>
    [Display(Name = "Main")]
    Main = 1 << 1, // 2

    /// <summary>
    /// Muscle control
    /// </summary>
    //[Display(Name = "Stability")]
    //Stability = 1 << 2, // 4

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Warmup/Cooldown")]
    WarmupCooldown = 1 << 3, // 8

    /// <summary>
    /// Primary mover of strength.
    /// </summary>
    //[Display(Name = "Super Strength")]
    //SuperStrength = 1 << 4, // 16
}
