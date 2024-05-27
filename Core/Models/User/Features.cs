using System.ComponentModel.DataAnnotations;

namespace Core.Models.User;

/// <summary>
/// Controls access to user features.
/// </summary>
[Flags]
public enum Features
{
    None = 0,

    [Display(Name = "Is a Demo User")]
    Demo = 1 << 0, // 1

    [Display(Name = "Is a Debug User")]
    Debug = 1 << 1, // 2

    [Display(Name = "Is a Test User")]
    Test = 1 << 2, // 4

    [Display(Name = "Is a LiveTest User")]
    LiveTest = 1 << 3, // 8

    /// <summary>
    /// Admin features.
    /// </summary>
    [Display(Name = "Admin Features")]
    Admin = 1 << 4, // 16

    /// <summary>
    /// Pre-beta features.
    /// </summary>
    [Display(Name = "View Alpha Features")]
    Alpha = 1 << 5, // 32

    /// <summary>
    /// Pre-prod features.
    /// </summary>
    [Display(Name = "View Beta Features")]
    Beta = 1 << 6, // 64
}
