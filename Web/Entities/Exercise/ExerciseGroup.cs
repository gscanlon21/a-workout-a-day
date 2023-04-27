using System.ComponentModel.DataAnnotations;

namespace Web.Entities.Exercise;

/// <summary>
/// Similar groups of exercises.
/// 
/// sa. Hand Planks and Forearm Planks should be in the same Planks group.
/// </summary>
[Flags]
public enum ExerciseGroup
{
    None = 0,

    [Display(Name = "Planks")]
    Planks = 1 << 0, // 1

    [Display(Name = "Handstands")]
    Handstands = 1 << 1, // 2

    //[Display(Name = "Side Planks")]
    //SidePlanks = 1 << 2, // 4
}
