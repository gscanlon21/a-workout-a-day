using Core.Code.Attributes;
using Core.Models.Exercise;
using System.Reflection;

namespace Core.Code.Extensions;

public static class PrehabFocusExtensions
{
    public static SkillsAttributeInternal? GetSkillType(this PrehabFocus rehabFocus)
    {
        var memberInfo = rehabFocus.GetType().GetMember(rehabFocus.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            return memberInfo[0].GetCustomAttribute(typeof(SkillsAttribute<>), true) as SkillsAttributeInternal;
        }

        return null;
    }
}
