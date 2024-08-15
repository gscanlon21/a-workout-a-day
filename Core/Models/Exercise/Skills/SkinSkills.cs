using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

[Flags]
public enum SkinSkills
{
    None = 0,

    [Display(Name = "Skin Protection")]
    SkinProtection = 1 << 0, // 1

    [Display(Name = "Skin Moisturization")]
    SkinMoisturization = 1 << 1, // 2

    [Display(Name = "Exfoliation")]
    Exfoliation = 1 << 2, // 4

    All = SkinProtection | SkinMoisturization | Exfoliation
}
