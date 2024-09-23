using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// The skill types that an exercise works.
/// 
/// Future-proofing with enum flags in case, say a neck exercise, helps with both eye skills and cervical skills (through head-tension reduction).
/// Note though, that none of the implementations using SkillTypes currently actually support multiple types.
/// </summary>
[Flags]
public enum SportsSkillTypes
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Pickleball Skills")]
    PickleballSkills = 1 << 0, // 1
}
