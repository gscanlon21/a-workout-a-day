using System.ComponentModel.DataAnnotations;

namespace Web.Data.Query.Options;

public class ProficiencyOptions
{
    /// <summary>
    ///     If true and the User's exercise proficiency is above the exercise's proficiency:
    ///     ... Will choose exercise that fall at or under the exercise's proficiency level.
    ///     Otherwise, will choose variations that fall within the User's exiercise progression range. 
    /// </summary>
    public bool DoCapAtProficiency { get; set; } = false;

    /// <summary>
    /// Adjusts the user's progression level of an exercise when calculating in-range progressions.
    /// 
    /// Removing this because if an exercise progression is already under the proficiency level then it's not difficult enough to need a deload.
    /// Also this didn't take into account exercise prerequisites.
    /// </summary>
    [Range(0, 1), Obsolete("Deprecated", error: true)]
    public double? CapAtUsersProficiencyPercent { get; set; } = null;
}