using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Anerobic exercise focus
/// </summary>
public enum ExerciseType
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Weight or resistance training. Anerobic.
    /// Anerobic.
    /// </summary>
    [Display(Name = "Resistance Training", ShortName = "Strengthening")]
    ResistanceTraining = 1,

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Stretching")]
    Stretching = 2,

    /// <summary>
    /// Stability training.
    /// </summary>
    [Display(Name = "Balance Training", ShortName = "Balance")]
    BalanceTraining = 3,

    /// <summary>
    /// Cardio. Aerobic.
    /// </summary>
    [Display(Name = "Cardiovasular Training", ShortName = "Cardio")]
    CardiovasularTraining = 4,

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Sports Training", ShortName = "Sports")]
    SportsTraining = 5,

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Injury Prevention", ShortName = "Prehab")]
    InjuryPrevention = 6,

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Rehabilitation", ShortName = "Rehab")]
    Rehabilitation = 7,

    /// <summary>
    /// Breating exercises.
    /// </summary>
    [Display(Name = "Mindfulness")]
    Mindfulness = 8,

    /// <summary>
    /// Core training.
    /// </summary>
    [Display(Name = "Core Training", ShortName = "Core")]
    CoreTraining = 9,
}
