using Core.Models.Exercise.Skills;

namespace Data.Query.Options;

public class SkillsOptions : IOptions
{
    public SkillsOptions() { }

    public SkillsOptions(Type? skillType, int? skills)
    {
        if (skills.HasValue)
        {
            switch (skillType)
            {
                case Type when skillType == typeof(VocalSkills): VocalSkills = (VocalSkills)skills; break;
                case Type when skillType == typeof(VisualSkills): VisualSkills = (VisualSkills)skills; break;
                case Type when skillType == typeof(CervicalSkills): CervicalSkills = (CervicalSkills)skills; break;
                case Type when skillType == typeof(ThoracicSkills): ThoracicSkills = (ThoracicSkills)skills; break;
                case Type when skillType == typeof(LumbarSkills): LumbarSkills = (LumbarSkills)skills; break;
            }
        }
    }

    public VocalSkills VocalSkills { get; }
    public VisualSkills VisualSkills { get; }
    public CervicalSkills CervicalSkills { get; }
    public ThoracicSkills ThoracicSkills { get; }
    public LumbarSkills LumbarSkills { get; }

    public bool RequireSkills { get; set; }

    public bool HasData() => VisualSkills != VisualSkills.None
        || CervicalSkills != CervicalSkills.None
        || ThoracicSkills != ThoracicSkills.None
        || LumbarSkills != LumbarSkills.None
        || VocalSkills != VocalSkills.None;
}
