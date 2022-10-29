using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise;

/// <summary>
/// Maintain/Obtain/Gain/Endurance/Recovery/WarmupCooldown
/// </summary>
public enum IntensityLevel
{
    [Display(Name = "Maintain")]
    Maintain = 0,

    [Display(Name = "Obtain")]
    Obtain = 1,

    [Display(Name = "Gain")]
    Gain = 2,

    [Display(Name = "Endurance")]
    Endurance = 3,

    [Display(Name = "Recovery")]
    Recovery = 4,

    [Display(Name = "Warmup/Cooldown")]
    WarmupCooldown = 5 // Might split these out at some point 
}
