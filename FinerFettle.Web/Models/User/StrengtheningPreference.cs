using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.User;

/// <summary>
/// Frequency of strengthing days
/// </summary>
public enum StrengtheningPreference
{
    /// <summary>
    /// Strength exercises are always full-body exercises.
    /// </summary>
    [Display(Name = "Maintain", Description = "Workouts will be low intensity.")]
    Maintain = 0,

    /// <summary>
    /// Strength exercises rotate between upper body, mid body, and lower body.
    /// </summary>
    [Display(Name = "Obtain", Description = "Workouts will be medium intensity.")]
    Obtain = 1,

    /// <summary>
    /// Strength exercises alternate between upper body and mid/lower body.
    /// </summary>
    [Display(Name = "Gain", Description = "Workouts will be high intensity.")]
    Gain = 2,
}
