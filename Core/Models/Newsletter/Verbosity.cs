using System.ComponentModel.DataAnnotations;

namespace Core.Models.Newsletter;

/// <summary>
/// The detail shown in the newsletter.
/// </summary>
[Flags]
public enum Verbosity
{
    None = 0,

    /// <summary>
    /// Show instructions to the user.
    /// </summary>
    [Display(Name = "Instructions")]
    Instructions = 1 << 0, // 1

    /// <summary>
    /// Show exercises images to the user.
    /// </summary>
    [Display(Name = "Images")]
    Images = 1 << 1, // 2

    /// <summary>
    /// Show the bottom progression bar to the user, 
    /// allowing them to progress and regress their exercise progression.
    /// </summary>
    [Display(Name = "Progression Bar")]
    ProgressionBar = 1 << 2, // 4

    /// <summary>
    /// Show which muscles are stretched by the exercise to the user.
    /// </summary>
    [Display(Name = "Stretched Muscles")]
    StretchMuscles = 1 << 3, // 8

    /// <summary>
    /// Show which muscles are strengthened by the exercise to the user.
    /// </summary>
    [Display(Name = "Strengthened Muscles")]
    StrengthMuscles = 1 << 4, // 16

    /// <summary>
    /// Show which muscles are partially strengthened by the exercise to the user.
    /// </summary>
    [Display(Name = "Secondary Muscles")]
    SecondaryMuscles = 1 << 5, // 32

    /// <summary>
    /// Show exercises set and rep ranges to the user.
    /// </summary>
    [Display(Name = "Proficiency")]
    Proficiency = 1 << 6, // 64

    /// <summary>
    /// This is not user-facing. 
    /// It should not have a Display attribute.
    /// </summary>
    All = Instructions | Images | ProgressionBar | StretchMuscles | StrengthMuscles | SecondaryMuscles | Proficiency
        | 1 << 30, // 1073741824

    /// <summary>
    /// This is not user-facing. 
    /// It should not have a Display attribute.
    /// </summary>
    Debug = Instructions | Images | ProgressionBar | StretchMuscles | StrengthMuscles | SecondaryMuscles | Proficiency | All
        | 1 << 31 // 2147483648
}
