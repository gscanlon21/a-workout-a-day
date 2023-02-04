using System.ComponentModel.DataAnnotations;
using Web.Models.Exercise;

namespace Web.Data.QueryBuilder;

public class ProficiencyOptions
{
    /// <summary>
    ///     If true and the User's exercise proficiency is above the exercise's proficiency:
    ///     ... Will choose exercise that fall at or under the exercise's proficiency level.
    ///     Otherwise, will choose variations that fall within the User's exiercise progression range. 
    /// </summary>
    public bool DoCapAtProficiency { get; set; } = false;

    /// <summary>
    /// If there is no exercise within the right progression range, do we allow easier variations?
    /// </summary>
    public bool AllowLesserProgressions { get; set; } = true;

    public bool FilterProgressions { get; set; } = true;

    // Removing this because if an exercise progression is already under the proficiency level then it's not difficult enough to need a deload
    //[Range(0, 1)]
    //public double? CapAtUsersProficiencyPercent { get; set; } = null;
}