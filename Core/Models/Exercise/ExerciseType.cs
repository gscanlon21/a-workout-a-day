using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// The training type.
/// 
/// ExerciseType filters are checked using flags values--these must be flags even if they are stored one-per in the db.
/// </summary>
[Flags]
public enum ExerciseType
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Cardio. Aerobic.
    /// </summary>
    [Display(Name = "Cardiovasular Training", ShortName = "Cardio")]
    CardiovasularTraining = 1 << 0, // 1

    /// <summary>
    /// Muscle range of motion and movement. Most stretches are included in this.
    /// </summary>
    [Display(Name = "Mobility Training", ShortName = "Mobility")]
    MobilityTraining = 1 << 1, // 2

    /// <summary>
    /// Weight or resistance training. Anerobic.
    /// Anerobic.
    /// </summary>
    [Display(Name = "Functional Training", ShortName = "Functional")]
    FunctionalTraining = 1 << 2, // 4

    /// <summary>
    /// Weight or resistance training. Anerobic.
    /// Anerobic.
    /// </summary>
    [Display(Name = "Accessory Training", ShortName = "Accessory")]
    AccessoryTraining = 1 << 3, // 8

    /// <summary>
    /// Weight or resistance training. Anerobic.
    /// Anerobic.
    /// </summary>
    [Display(Name = "Resistance Training", ShortName = "Resistance")]
    ResistanceTraining = AccessoryTraining | FunctionalTraining, // 12

    /// <summary>
    /// Core training.
    /// </summary>
    [Display(Name = "Core Training", ShortName = "Core")]
    CoreTraining = 1 << 4, // 16

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Sports Training", ShortName = "Sports")]
    SportsTraining = 1 << 5, // 32

    /// <summary>
    /// Stability training.
    /// </summary>
    [Display(Name = "Balance Training", ShortName = "Balance")]
    BalanceTraining = 1 << 6, // 64

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Injury Prevention", ShortName = "Prehab")]
    InjuryPrevention = 1 << 7, // 128 

    /// <summary>
    /// Is eligible to be viewed by sports or recovery tracks.
    /// </summary>
    [Display(Name = "Rehabilitation", ShortName = "Rehab")]
    Rehabilitation = 1 << 8, // 256

    /// <summary>
    /// Breating exercises.
    /// </summary>
    [Display(Name = "Mindfulness")]
    Mindfulness = 1 << 9, // 512

    All = CardiovasularTraining | MobilityTraining | FunctionalTraining | AccessoryTraining | CoreTraining | SportsTraining | BalanceTraining | InjuryPrevention | Rehabilitation | Mindfulness
}
