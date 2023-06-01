using System.ComponentModel.DataAnnotations;

namespace Web.Models.Footnote;

[Flags]
public enum FootnoteType
{
    None = 0,

    /// <summary>
    /// Take five to 10 minutes to warm up and cool down properly.
    /// </summary>
    [Display(Name = "Fitness Advice", Description = "sa. Take five to 10 minutes to warm up and cool down properly.")]
    FitnessAdvice = 1 << 0, // 1

    /// <summary>
    /// You are beautiful!
    /// </summary>
    [Display(Name = "Inspirations", Description = "sa. You are beautiful!")]
    Inspirations = 1 << 1, // 2

    /// <summary>
    /// I'm a thoughtful and interesting person.
    /// </summary>
    [Display(Name = "Affirmations", Description = "sa. I'm a thoughtful and interesting person.")]
    Affirmations = 1 << 2, // 4

    All = FitnessAdvice | Inspirations | Affirmations
}
