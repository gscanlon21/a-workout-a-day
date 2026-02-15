using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// Sports that the user will see additional exercises for.
/// We don't want to get granular with Sports Skills.
/// That would be too tedius. Refresh padding works.
/// </summary>
[Flags]
public enum SportsFocus
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Tennis")]
    Tennis = 1 << 0, // 1

    [Display(Name = "Soccer")]
    Soccer = 1 << 1, // 2

    [Display(Name = "Hockey")]
    Hockey = 1 << 2, // 4

    [Display(Name = "Baseball")]
    Baseball = 1 << 3, // 8

    [Display(Name = "Boxing")]
    Boxing = 1 << 4, // 16

    [Display(Name = "Football")]
    Football = 1 << 5, // 32

    [Display(Name = "Basketball")]
    Basketball = 1 << 6, // 64

    [Display(Name = "Pickleball")]
    Pickleball = 1 << 7, // 128

    [Display(Name = "Volleyball")]
    Volleyball = 1 << 8, // 256

    [Display(Name = "Cricket")]
    Cricket = 1 << 9, // 512

    [Display(Name = "Rugby")]
    Rugby = 1 << 10, // 1024

    [Display(Name = "Lacrosse")]
    Lacrosse = 1 << 11, // 2048

    [Display(Name = "Frisbee")]
    Frisbee = 1 << 12, // 4096

    [Display(Name = "All")]
    All = Tennis | Soccer | Hockey | Baseball | Boxing | Football | Basketball | Pickleball | Volleyball | Cricket | Rugby | Lacrosse | Frisbee
}
