using Core.Models.Exercise.Skills;

namespace Core.Code.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SportsSkillsAttribute<T> : SportsSkillsAttributeInternal where T : struct, Enum
{
    public SportsSkillsAttribute(SportsSkillTypes skillType)
    {
        SkillType = skillType;
    }

    public override SportsSkillTypes SkillType { get; }
    public override Type Type { get; } = typeof(T);
    public override Enum[] AllValues => Enum.GetValues<T>().Select(m => (Enum)m).ToArray();
    public override Enum[] SelectList => EnumExtensions.GetDisplayValues<T>().Select(m => (Enum)m).ToArray();
}

public abstract class SportsSkillsAttributeInternal : Attribute
{
    public abstract Type Type { get; }
    public abstract Enum[] AllValues { get; }
    public abstract Enum[] SelectList { get; }
    public abstract SportsSkillTypes SkillType { get; }
}