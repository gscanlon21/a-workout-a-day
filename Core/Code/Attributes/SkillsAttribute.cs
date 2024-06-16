using Core.Code.Extensions;

namespace Core.Code.Attributes;


/// <summary>
/// Provides conditional validation based on related property value.
/// </summary>
/// <param name="otherProperty">The other property.</param>
/// <param name="otherPropertyValue">The other property value.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SkillsAttribute<T>() : SkillsAttributeInternal where T : struct, Enum
{
    /// <summary>
    /// The other property name that will be used during validation.
    /// </summary>
    public Type SkillType { get; private set; } = typeof(T);

    public override Enum[] SelectList => EnumExtensions.GetDisplayValues<T>().Select(m => (Enum)m).ToArray();
}

public abstract class SkillsAttributeInternal : Attribute
{
    public abstract Enum[] SelectList { get; }
}