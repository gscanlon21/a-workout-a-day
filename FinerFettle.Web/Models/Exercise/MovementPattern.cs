using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.Models.Exercise;

[Flags]
public enum MovementPattern
{
    None = 0,

    [Display(Name = "Horizontal Push", GroupName = "Upper Body Push")]
    HorizontalPush = 1 << 0, // 1

    [Display(Name = "Horizontal Pull", GroupName = "Upper Body Pull")]
    HorizontalPull = 1 << 1, // 2

    [Display(Name = "Vertical Push", GroupName = "Upper Body Push")]
    VerticalPush = 1 << 2, // 4

    [Display(Name = "Vertical Pull", GroupName = "Upper Body Pull")]
    VerticalPull = 1 << 3, // 8

    [Display(Name = "Upper Body Pull")]
    UpperBodyPull = HorizontalPull | VerticalPull,

    [Display(Name = "Upper Body Push")]
    UpperBodyPush = HorizontalPush | VerticalPush,

    [Display(Name = "Hip Hinge")]
    HipHinge = 1 << 4, // 16

    [Display(Name = "Squat")]
    Squat = 1 << 5, // 32

    [Display(Name = "Lunge")]
    Lunge = 1 << 6, // 64

    [Display(Name = "Carry")]
    Carry = 1 << 7, // 128

    [Display(Name = "Rotation")]
    Rotation = 1 << 8 // 256
}
