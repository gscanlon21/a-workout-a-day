using Core.Code.Attributes;
using Core.Models.Exercise;
using System.Reflection;

namespace Core.Code.Extensions;

public static class RehabFocusExtensions
{
    public static SkillsAttributeInternal? GetSkillType(this RehabFocus rehabFocus)
    {
        var memberInfo = rehabFocus.GetType().GetMember(rehabFocus.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            var attrs = memberInfo[0].GetCustomAttribute(typeof(SkillsAttribute<>), true) as SkillsAttributeInternal;
            return attrs;
        }

        return null;
    }
}
