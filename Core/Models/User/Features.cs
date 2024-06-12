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

    /// <summary>
    /// Unhandled exception emails.
    /// </summary>
    [Display(Name = "Dev")]
    Dev = 1 << 3, // 8

    /// <summary>
    /// Admin / prerelease features.
    /// </summary>
    [Display(Name = "Admin")]
    Admin = 1 << 4, // 16

    /// <summary>
    /// Pre-beta features.
    /// </summary>
    [Display(Name = "Alpha")]
    Alpha = 1 << 5, // 32

    /// <summary>
    /// Pre-prod features.
    /// </summary>
    [Display(Name = "Beta")]
    Beta = 1 << 6, // 64
}
