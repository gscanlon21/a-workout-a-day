using System.ComponentModel.DataAnnotations;

namespace Core.Models.Exercise.Skills;

/// <summary>
/// What type of group is this?
/// </summary>
public enum SkillTypes
{
    [Display(Name = "Workout Skills")]
    WorkoutSkills = 0,

    [Display(Name = "Visual Skills")]
    VisualSkills = 1,
}
