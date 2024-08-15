using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

[Flags]
public enum SkinSkills
{
    None = 0,

    [Display(Name = "Cleansing")]
    Cleansing = 1 << 0, // 1

    [Display(Name = "Toner")]
    Toner = 1 << 1, // 2

    [Display(Name = "Serum")]
    Serum = 1 << 2, // 4

    [Display(Name = "Eye Cream")]
    EyeCream = 1 << 3, // 8

    [Display(Name = "Moisturizer")]
    Moisturizer = 1 << 4, // 16

    [Display(Name = "Sunscreen")]
    Sunscreen = 1 << 5, // 32

    All = Cleansing | Toner | Serum | EyeCream | Moisturizer | Sunscreen
}
