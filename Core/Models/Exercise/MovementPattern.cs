using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise;

/// <summary>
/// https://www.aworkoutroutine.com/movement-patterns/
/// </summary>
[Flags]
public enum MovementPattern
{
    [Display(Name = "Accessory")]
    None = 0,

    //[Display(GroupName = "Push", Name = "Horizontal Push")]
    //HorizontalPush = 1 << 0, // 1

    //[Display(GroupName = "Pull", Name = "Horizontal Pull")]
    //HorizontalPull = 1 << 1, // 2

    //[Display(GroupName = "Push", Name = "Vertical Push")]
    //VerticalPush = 1 << 2, // 4

    //[Display(GroupName = "Pull", Name = "Vertical Pull")]
    //VerticalPull = 1 << 3, // 8

    //[Display(Name = "Push")]
    //Push = 1 << 0 | 1 << 2,

    //[Display(Name = "Pull")]
    //Pull = 1 << 1 | 1 << 3,

    /// <summary>
    /// Hip/Hamstring Dominant Exercises.
    /// </summary>
    [Display(Name = "Hip Extension")]
    HipExtension = 1 << 4, // 16

    //[Display(GroupName = "Knee Flexion", Name = "Squat")]
    //Squat = 1 << 5, // 32

    //[Display(GroupName = "Knee Flexion", Name = "Lunge")]
    //Lunge = 1 << 6, // 64

    //[Display(Name = "Knee Flexion")]
    //KneeFlexion = 1 << 5 | 1 << 6,

    [Display(Name = "Carry")]
    Carry = 1 << 7, // 128

    [Display(Name = "Rotation")]
    Rotation = 1 << 8, // 256

    [Display(GroupName = "Push", Name = "Horizontal Push")]
    HorizontalPush = 1 << 9 | 1 << 0, // 1

    [Display(GroupName = "Pull", Name = "Horizontal Pull")]
    HorizontalPull = 1 << 10 | 1 << 1, // 2

    [Display(GroupName = "Push", Name = "Vertical Push")]
    VerticalPush = 1 << 11 | 1 << 2, // 4

    [Display(GroupName = "Pull", Name = "Vertical Pull")]
    VerticalPull = 1 << 12 | 1 << 3, // 8

    /// <summary>
    /// Along the horizontal axis. Lateral. Legs are even with each other.
    /// 
    /// Includes Lateral Squats and Lateral Lunges.
    /// </summary>
    [Display(GroupName = "Knee Flexion", Name = "Squat")]
    Squat = 1 << 13 | 1 << 5, // 32

    /// <summary>
    /// Along the depth axis. Longitudinal. A forwards/backwards planar movement. Legs are staggered.
    /// 
    /// Includes Lunges and Split Squats.
    /// </summary>
    [Display(GroupName = "Knee Flexion", Name = "Lunge")]
    Lunge = 1 << 14 | 1 << 6, // 64

    [Display(Name = "Push")]
    Push = 1 << 15 | 1 << 0 | 1 << 2,

    [Display(Name = "Pull")]
    Pull = 1 << 16 | 1 << 1 | 1 << 3,

    /// <summary>
    /// Quad Dominant Exercises.
    /// </summary>
    [Display(Name = "Knee Flexion")]
    KneeFlexion = 1 << 17 | 1 << 5 | 1 << 6,

    All = KneeFlexion | Pull | Push | Rotation | Carry | HipExtension
}
