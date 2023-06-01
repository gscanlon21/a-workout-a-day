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
    /// Practicing everyday mindfulness can improve your memory and concentration skills....
    /// </summary>
    [Display(Name = "Life Advice", Description = "sa. Practicing everyday mindfulness can improve your memory and concentration skills...")]
    LifeAdvice = 1 << 1, // 2

    /// <summary>
    /// Never give up!
    /// </summary>
    [Display(Name = "Fitness Motivation", Description = "sa. Never give up!")]
    FitnessMotivation = 1 << 2, // 4

    /// <summary>
    /// Never give up!
    /// </summary>
    [Display(Name = "Life Motivation", Description = "sa. Never give up!")]
    LifeMotivation = 1 << 3, // 8

    /// <summary>
    /// I'm getting stronger after every workout
    /// </summary>
    [Display(Name = "Fitness Affirmations", Description = "sa. I'm getting stronger after every workout.")]
    FitnessAffirmations = 1 << 4, // 16

    /// <summary>
    /// I'm a thoughtful and interesting person.
    /// </summary>
    [Display(Name = "Life Affirmations", Description = "sa. I'm a thoughtful and interesting person.")]
    LifeAffirmations = 1 << 5, // 32

    /// <summary>
    /// You are beautiful!
    /// </summary>
    [Display(Name = "Good Vibes", Description = "sa. You are beautiful!")]
    GoodVibes = 1 << 6, // 64

    Top = All & ~Bottom, // FitnessAffirmations | LifeAffirmations,

    Bottom = FitnessAdvice | FitnessMotivation | LifeAdvice | LifeMotivation | GoodVibes,

    All = FitnessAdvice | FitnessMotivation | FitnessAffirmations | LifeAdvice | LifeMotivation | LifeAffirmations | GoodVibes
}
