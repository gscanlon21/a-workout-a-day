using System.ComponentModel.DataAnnotations;

namespace Lib.Models.Equipment;

/// <summary>
/// A location that weights are held during an exercise.
/// </summary>
public enum EquipmentLocation
{
    /// <summary>
    /// Holding the weight down near the hips.
    /// </summary>
    [Display(Name = "Suitcase")]
    Suitcase = 1,

    /// <summary>
    /// Front racked. Holding the weights (one in each hand) up near the shoulders.
    /// 
    /// aka. Rack.
    /// </summary>
    [Display(Name = "Front")]
    Front = 2,

    /// <summary>
    /// Holding the weights behind your neck, up near your shoulders.
    /// </summary>
    [Display(Name = "Back")]
    Back = 3,

    /// <summary>
    /// Holding the weights overhead.
    /// </summary>
    [Display(Name = "Overhead")]
    Overhead = 4,

    /// <summary>
    /// Holding one weight up near the chest.
    /// </summary>
    [Display(Name = "Goblet")]
    Goblet = 5,

    /// <summary>
    /// Holding the weights down near your hips, but behind your back.
    /// </summary>
    [Display(Name = "Hack")]
    Hack = 6,

    [Display(Name = "Flexion")]
    Flexion = 7,

    [Display(Name = "Extension")]
    Extension = 8,

    [Display(Name = "Pronation")]
    Pronation = 9,

    [Display(Name = "Supination")]
    Supination = 10,

    [Display(Name = "Reverse Grip")]
    ReverseGrip = 11,

    [Display(Name = "Close Grip")]
    CloseGrip = 12,

    [Display(Name = "Neutral Grip")]
    NeutralGrip = 13,

    [Display(Name = "Wide Grip")]
    WideGrip = 14,

    [Display(Name = "Incline")]
    Incline = 15,

    [Display(Name = "Flat")]
    Flat = 16,

    [Display(Name = "Decline")]
    Decline = 17,

    [Display(Name = "Normal Grip")]
    NormalGrip = 18,
}
