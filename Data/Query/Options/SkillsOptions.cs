using Core.Models.Exercise.Skills;

namespace Data.Query.Options;

public class SkillsOptions : IOptions
{
    public SkillsOptions() { }

    public SkillsOptions(SkillTypes skillType, int? skills)
    {
        SkillType = skillType;
        Skills = skills;
    }

    public SkillTypes SkillType { get; set; }
    public int? Skills { get; set; }

    public bool RequireSkills { get; set; }

    public bool HasData()
    {
        return SkillType != SkillTypes.None && Skills.HasValue && Skills.Value > 0;
    }
}
