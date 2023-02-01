using System.ComponentModel.DataAnnotations;

namespace Web.Models.User;

/// <summary>
/// How intense workouts will be.
/// </summary>
public enum StrengtheningPreference
{
    /// <summary>
    /// The target range for muscle failure will consist of few sets of higher reps—ideal for lifting lighter weights and building muscle endurance.
    /// </summary>
    [Display(Name = "Light", Description = "The target range for muscle failure will consist of few sets of higher reps—ideal for lifting lighter weights and building muscle endurance.")]
    Light = 0,

    /// <summary>
    /// The target range for muscle failure will consist of a medial number of sets and rep—ideal for lifting medium weights and building muscle mass.
    /// </summary>
    [Display(Name = "Medium", Description = "The target range for muscle failure will consist of a medial number of sets and rep—ideal for lifting medium weights and building muscle mass.")]
    Medium = 1,

    /// <summary>
    /// The target range for muscle failure will consist of many sets of lower reps—ideal for lifting heavy weights and building muscle strength.
    /// </summary>
    [Display(Name = "Heavy", Description = "The target range for muscle failure will consist of many sets of lower reps—ideal for lifting heavy weights and building muscle strength.")]
    Heavy = 2,
}
