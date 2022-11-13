using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Web.Models.User;

/// <summary>
/// Frequency of strengthing days
/// </summary>
public enum Frequency
{
    /// <summary>
    /// Workouts work the whole body each day.
    /// </summary>
    [Display(Name = "Full-Body", Description = "Workouts will work the whole body.")]
    FullBody = 0,

    /// <summary>
    /// Workouts are split into two upper/lower body days.
    /// </summary>
    [Display(Name = "Upper/Lower", Description = "Workouts will be split into upper/lower body days.")]
    UpperLowerBodySplit = 1,
}
