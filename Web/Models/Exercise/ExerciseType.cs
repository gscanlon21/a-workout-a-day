using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// <summary>
/// Anerobic exercise focus
/// </summary>
[Flags]
public enum ExerciseType
{
    /// <summary>
    /// Weight or resistance training. Anerobic.
    /// Anerobic.
    /// </summary>
    [Display(Name = "Resistance Training", ShortName = "Strengthening")]
    ResistanceTraining = 1 << 0, // 1

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Stretching")]
    Stretching = 1 << 1, // 2

    /// <summary>
    /// Stability training.
    /// </summary>
    [Display(Name = "Balance Training", ShortName = "Balance")]
    BalanceTraining = 1 << 2, // 4

    /// <summary>
    /// Cardio. Aerobic.
    /// </summary>
    [Display(Name = "Cardiovasular Training", ShortName = "Cardio")]
    CardiovasularTraining = 1 << 3, // 8

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Sports Training", ShortName = "Sports")]
    SportsTraining = 1 << 4, // 16

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Injury Prevention", ShortName = "Prehab")]
    InjuryPrevention = 1 << 5, // 32

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Rehabilitation", ShortName = "Rehab")]
    Rehabilitation = 1 << 6, // 64

    All = ResistanceTraining | Stretching | BalanceTraining | CardiovasularTraining | SportsTraining | InjuryPrevention | Rehabilitation
}
