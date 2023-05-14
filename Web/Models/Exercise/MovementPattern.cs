using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

/// REFACTOR: This should not be an enum
[Flags]
public enum MovementPattern
{
    [Display(Name = "None")]
    None = 0,

    //[Display(Name = "Horizontal Push", GroupName = "Push")]
    //HorizontalPush = 1 << 0, // 1

    //[Display(Name = "Horizontal Pull", GroupName = "Pull")]
    //HorizontalPull = 1 << 1, // 2

    //[Display(Name = "Vertical Push", GroupName = "Push")]
    //VerticalPush = 1 << 2, // 4

    //[Display(Name = "Vertical Pull", GroupName = "Pull")]
    //VerticalPull = 1 << 3, // 8

    //[Display(Name = "Push")]
    //Push = 1 << 0 | 1 << 2,

    //[Display(Name = "Pull")]
    //Pull = 1 << 1 | 1 << 3,

    [Display(Name = "Hip Extension")]
    HipExtension = 1 << 4, // 16

    /// <summary>
    /// Along the horizontal axis. Lateral. Legs are even with each other.
    /// 
    /// Includes Lateral Squats and Lateral Lunges.
    /// </summary>
    //[Display(Name = "Squat", GroupName = "Knee Flexion")]
    //Squat = 1 << 5, // 32

    /// <summary>
    /// Along the depth axis. Longitudinal. A forwards/backwards planar movement. Legs are staggered.
    /// 
    /// Includes Lunges and Split Squats.
    /// </summary>
    //[Display(Name = "Lunge", GroupName = "Knee Flexion")]
    //Lunge = 1 << 6, // 64

    //[Display(Name = "Knee Flexion")]
    //KneeFlexion = 1 << 5 | 1 << 6,

    [Display(Name = "Carry")]
    Carry = 1 << 7, // 128

    [Display(Name = "Rotation")]
    Rotation = 1 << 8, // 256

    [Display(Name = "Horizontal Push", GroupName = "Push")]
    HorizontalPush = 1 << 9 | 1 << 0, // 1

    [Display(Name = "Horizontal Pull", GroupName = "Pull")]
    HorizontalPull = 1 << 10 | 1 << 1, // 2

    [Display(Name = "Vertical Push", GroupName = "Push")]
    VerticalPush = 1 << 11 | 1 << 2, // 4

    [Display(Name = "Vertical Pull", GroupName = "Pull")]
    VerticalPull = 1 << 12 | 1 << 3, // 8

    /// <summary>
    /// Along the horizontal axis. Lateral. Legs are even with each other.
    /// 
    /// Includes Lateral Squats and Lateral Lunges.
    /// </summary>
    [Display(Name = "Squat", GroupName = "Knee Flexion")]
    Squat = 1 << 13 | 1 << 5, // 32

    /// <summary>
    /// Along the depth axis. Longitudinal. A forwards/backwards planar movement. Legs are staggered.
    /// 
    /// Includes Lunges and Split Squats.
    /// </summary>
    [Display(Name = "Lunge", GroupName = "Knee Flexion")]
    Lunge = 1 << 14 | 1 << 6, // 64

    [Display(Name = "Push")]
    Push = 1 << 15 | 1 << 0 | 1 << 2,

    [Display(Name = "Pull")]
    Pull = 1 << 16 | 1 << 1 | 1 << 3,

    [Display(Name = "Knee Flexion")]
    KneeFlexion = 1 << 17 | 1 << 5 | 1 << 6,

    All = KneeFlexion | Pull | Push | Rotation | Carry | HipExtension
}
