using System.ComponentModel.DataAnnotations;

namespace Web.Models.User;

/// <summary>
/// Sports that the user will see additional exercises for.
/// </summary>
[Flags]
public enum SportsFocus
{
    [Display(Name = "None")]
    None = 1 << 0, // 1

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

    [Display(Name = "Tennis")]
    Tennis = 1 << 8, // 256
}
