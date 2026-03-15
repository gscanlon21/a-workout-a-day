using System.ComponentModel.DataAnnotations;

namespace Core.Models.User;

/// <summary>
/// Frequency of strengthening days
/// </summary>
public enum Frequency
{
    [Display(Name = "None")]
    None = -1,

    /// <summary>
    /// Mobility/stretching exercises.
    /// </summary>
    [Display(Name = "Mobility", ShortName = "Mobility", Description = "Rest-Day mobility workouts.")]
    Mobility = 0,

    /// <summary>
    /// Workouts will work the whole body. Functional movement patterns will be split between two days.
    /// </summary>
    [Display(Name = "2-Day Full-Body", ShortName = "Full-Body", Description = "Workouts will work the whole body. Functional movement patterns will be split between two days.", Order = 1)]
    FullBody2Day = 1,

    /// <summary>
    /// Workouts will be split into one of three push/pull/legs days. Functional movement patterns will be split between three days.
    /// </summary>
    [Display(Name = "3-Day Push/Pull/Legs", ShortName = "Push/Pull/Legs", Description = "Workouts will be split into one of three push/pull/legs days. Functional movement patterns will be split between three days.", Order = 2)]
    PushPullLeg3Day = 2,

    /// <summary>
    /// Workouts will be split into one of two upper/lower body days. Functional movement patterns will be split between four days.
    /// </summary>
    [Display(Name = "4-Day Upper/Lower", ShortName = "Upper/Lower", Description = "Workouts will be split into one of two upper/lower body days. Functional movement patterns will be split between four days.", Order = 4)]
    UpperLowerBodySplit4Day = 3,

    /// <summary>
    /// Combination of the Upper/Lower split and Full-Body workouts.
    /// </summary>
    [Display(Name = "3-Day Upper/Lower/Full-Body", ShortName = "Upper/Lower/Full-Body", Description = "Combination of the Upper/Lower split and Full-Body workouts.", Order = 3)]
    UpperLowerFullBodySplit3Day = 4,

    /// <summary>
    // Combination of the Push/Pull/Legs split and Full-Body workouts.
    /// </summary>
    [Display(Name = "4-Day Push/Pull/Legs/Full-Body", ShortName = "Push/Pull/Legs/Full-Body", Description = "Combination of the Push/Pull/Legs split and Full-Body workouts.", Order = 5)]
    PushPullLegsFullBodySplit4Day = 5,

    /// <summary>
    /// Combination of the Push/Pull/Legs and Upper/Lower splits.
    /// </summary>
    [Display(Name = "5-Day Push/Pull/Legs/Upper/Lower", ShortName = "Push/Pull/Legs/Upper/Lower", Description = "Combination of the Push/Pull/Legs and Upper/Lower splits.", Order = 6)]
    PushPullLegsUpperLowerSplit5Day = 6,

    /// <summary>
    /// A split that focuses on variety more than strengthening.
    /// </summary>
    [Display(Name = "6-Day Variety", ShortName = "6-Day Variety", Description = "A workout split that focuses on exercise variety.", Order = 7)]
    VarietySplit6Day = 7,

    /// <summary>
    /// A split that focuses on variety more than strengthening.
    /// </summary>
    [Display(Name = "3-Day Variety", ShortName = "3-Day Variety", Description = "A workout split that focuses on exercise variety.", Order = 8)]
    VarietySplit3Day = 8,

    /// <summary>
    /// Allows the user to customize their workout split.
    /// </summary>
    [Display(Name = "Custom", ShortName = "Custom", Description = "Create a custom workout split.", Order = 9)]
    Custom = 9,
}
