using System.ComponentModel.DataAnnotations;

namespace Web.Models.User;

[Flags]
public enum Bonus
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Advanced Calisthenics", Description = "Variations such as One-Arm Pullups")]
    AdvancedCalisthenics = 1 << 0, // 1

    [Display(Name = "Advanced Weight Lifting", Description = "Variations such as the Clean and Press")]
    AdvancedWeightLifting = 1 << 1, // 2

    [Display(Name = "All")]
    All = AdvancedCalisthenics | AdvancedWeightLifting
}
