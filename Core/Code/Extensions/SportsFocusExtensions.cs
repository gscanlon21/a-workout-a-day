using Core.Code.Attributes;
using Core.Models.Exercise;
using System.Reflection;

namespace Core.Code.Extensions;

public static class SportsFocusExtensions
{
    public static SportsSkillsAttributeInternal? GetSkillType(this SportsFocus rehabFocus)
    {
        var memberInfo = rehabFocus.GetType().GetMember(rehabFocus.ToString());
        if (memberInfo != null && memberInfo.Length > 0)
        {
            return memberInfo[0].GetCustomAttribute(typeof(SportsSkillsAttribute<>), true) as SportsSkillsAttributeInternal;
        }

        return null;
    }
}
