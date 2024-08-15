using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// What type of group is this?
/// </summary>
public enum SkillTypes
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Visual Skills")]
    VisualSkills = 1,

    [Display(Name = "Skin Skills")]
    SkinSkills = 1,
}
