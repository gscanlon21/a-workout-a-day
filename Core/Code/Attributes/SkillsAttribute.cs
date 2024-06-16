using Core.Code.Extensions;

namespace Core.Code.Attributes;


/// <summary>
/// Provides conditional validation based on related property value.
/// </summary>
/// <param name="otherProperty">The other property.</param>
/// <param name="otherPropertyValue">The other property value.</param>
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public sealed class SkillsAttribute<T>() : SkillsAttributeInternal where T : Enum
{
    /// <summary>
    /// The other property name that will be used during validation.
    /// </summary>
    public Type SkillType { get; private set; } = typeof(T);

    public override Enum[] SelectList => EnumExtensions.GetSingleOrNoneValues32<T>().Cast<Enum>().ToArray();
}

public abstract class SkillsAttributeInternal : Attribute
{
    public abstract Enum[] SelectList { get; }
}