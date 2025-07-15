using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// The skill types that an exercise works.
/// 
/// Future-proofing with enum flags in case, say a neck exercise, helps with both eye skills and cervical skills (through head-tension reduction).
/// Note though, that none of the implementations using SkillTypes currently actually support multiple types.
/// </summary>
[Flags]
public enum SkillTypes
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Visual Skills")]
    VisualSkills = 1 << 0, // 1

    [Display(Name = "Cervical Skills")]
    CervicalSkills = 1 << 1, // 2

    [Display(Name = "Thoracic Skills")]
    ThoracicSkills = 1 << 2, // 4

    [Display(Name = "Lumbar Skills")]
    LumbarSkills = 1 << 3, // 8
}
