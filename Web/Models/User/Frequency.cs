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
    [Display(Name = "2-Day Full-Body", Description = "Workouts will work the whole body. Functional movement patterns will be split between two days.")]
    FullBody2Day = 0,

    /// <summary>
    /// Workouts are split into two upper/lower body days.
    /// </summary>
    [Display(Name = "4-Day Upper/Lower", Description = "Workouts will be split into one of two upper/lower body days. Functional movement patterns will be split between four days.")]
    UpperLowerBodySplit4Day = 1,

    /// <summary>
    /// Workouts are split into two upper/lower body days.
    /// </summary>
    [Display(Name = "2-Day Upper/Lower", Description = "Workouts will be split into one of two upper/lower body days. Functional movement patterns will be split between two days.")]
    UpperLowerBodySplit2Day = 2,

    /// <summary>
    /// Workouts are split into two upper/lower body days.
    /// </summary>
    [Display(Name = "3-Day Push/Pull/Legs", Description = "Workouts will be split into one of three push/pull/legs days. Functional movement patterns will be split between three days.")]
    PushPullLeg3Day = 3,
}
