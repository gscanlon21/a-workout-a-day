using System.ComponentModel.DataAnnotations;

namespace Web.Models.User;

[Flags]
public enum Features
{
    None = 0,

    [Display(Name = "Can View Demo Features")]
    Demo = 1 << 0, // 1

    [Display(Name = "Can See Alpha Features")]
    Alpha = 1 << 1, // 2

    [Display(Name = "Can See Beta Features")]
    Beta = 1 << 2, // 4

    [Display(Name = "Can View Debug Features")]
    Debug = 1 << 3, // 8

    [Display(Name = "Can Receive Multiple Emails Each Day")]
    ManyEmails = 1 << 4, // 16
}
