using System.ComponentModel.DataAnnotations;

namespace Core.Models.User;

/// <summary>
/// Controls access to user features.
/// </summary>
[Flags]
public enum Features
{
    None = 0,

    [Display(Name = "Demo")]
    Demo = 1 << 0, // 1

    [Display(Name = "Debug")]
    Debug = 1 << 1, // 2

    [Display(Name = "Test")]
    Test = 1 << 2, // 4

    [Display(Name = "Admin")]
    Admin = 1 << 3, // 8
}
