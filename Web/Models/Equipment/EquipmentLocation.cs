using System.ComponentModel.DataAnnotations;

namespace Web.Models.Equipment;

/// <summary>
/// A location that weights are held during an exercise.
/// </summary>
public enum EquipmentLocation
{
    [Display(Name = "Suitcase")]
    Suitcase = 1,

    [Display(Name = "Front")]
    Front = 2,

    [Display(Name = "Back")]
    Back = 3,

    [Display(Name = "Overhead")]
    Overhead = 4,

    [Display(Name = "Goblet")]
    Goblet = 5,

    [Display(Name = "Rack")]
    Rack = 6,

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
}
