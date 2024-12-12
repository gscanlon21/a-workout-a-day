using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

[Flags]
public enum MuscleMovement
{
    [Display(Name = "Static")]
    Static = 1 << 0, // 1

    [Display(Name = "Dynamic")]
    Dynamic = 1 << 1, // 2

    All = Static | Dynamic
}
