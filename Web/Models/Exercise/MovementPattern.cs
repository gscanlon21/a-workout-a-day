using System.ComponentModel.DataAnnotations;

namespace Web.Models.Exercise;

[Flags]
public enum MovementPattern
{
    None = 0,

    [Display(Name = "Horizontal Push", GroupName = "Push")]
    HorizontalPush = 1 << 0, // 1

    [Display(Name = "Horizontal Pull", GroupName = "Pull")]
    HorizontalPull = 1 << 1, // 2

    [Display(Name = "Vertical Push", GroupName = "Push")]
    VerticalPush = 1 << 2, // 4

    [Display(Name = "Vertical Pull", GroupName = "Pull")]
    VerticalPull = 1 << 3, // 8

    [Display(Name = "Pull")]
    Pull = HorizontalPull | VerticalPull,

    [Display(Name = "Push")]
    Push = HorizontalPush | VerticalPush,

    [Display(Name = "Hip Extension")]
    HipExtension = 1 << 4, // 16

    /// <summary>
    /// Along the horizontal axis. Lateral. Legs are even with each other.
    /// 
    /// Includes Lateral Squats and Lateral Lunges.
    /// </summary>
    [Display(Name = "Squat", GroupName = "Knee Flexion")]
    Squat = 1 << 5, // 32

    /// <summary>
    /// Along the depth axis. Longitudinal. A forwards/backwards planar movement. Legs are staggered.
    /// 
    /// Includes Lunges and Split Squats.
    /// </summary>
    [Display(Name = "Lunge", GroupName = "Knee Flexion")]
    Lunge = 1 << 6, // 64

    [Display(Name = "Knee Flexion")]
    KneeFlexion = Squat | Lunge,

    [Display(Name = "Carry")]
    Carry = 1 << 7, // 128

    [Display(Name = "Rotation")]
    Rotation = 1 << 8 // 256
}
