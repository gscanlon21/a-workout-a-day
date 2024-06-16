
namespace Data.Query.Options;

public class SkillsOptions : IOptions
{
    public SkillsOptions() { }

    public SkillsOptions(int skills)
    {
        Skills = skills;
    }

    public int? Skills { get; set; }
}
