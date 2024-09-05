using Core.Models.Exercise.Skills;

namespace Core.Code.Attributes;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SkillsAttribute<T> : SkillsAttributeInternal where T : struct, Enum
{
    public SkillsAttribute(SkillTypes skillType)
    {
        SkillType = skillType;
    }

    public override SkillTypes SkillType { get; }
    public override Type Type { get; } = typeof(T);
    public override Enum[] AllValues => Enum.GetValues<T>().Select(m => (Enum)m).ToArray();
    public override Enum[] SelectList => EnumExtensions.GetDisplayValues<T>().Select(m => (Enum)m).ToArray();
}

public abstract class SkillsAttributeInternal : Attribute
{
    public abstract Type Type { get; }
    public abstract Enum[] AllValues { get; }
    public abstract Enum[] SelectList { get; }
    public abstract SkillTypes SkillType { get; }
}