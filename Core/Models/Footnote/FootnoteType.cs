using System.ComponentModel.DataAnnotations;

namespace Core.Models.Footnote;

[Flags]
public enum FootnoteType
{
    None = 0,

    /// <summary>
    /// Fitness advice and tips. 
    /// 
    /// sa. Take five to 10 minutes to warm up and cool down properly.
    /// </summary>
    [Display(Name = "Fitness Advice", Description = "sa. Take five to 10 minutes to warm up and cool down properly.")]
    FitnessAdvice = 1 << 0, // 1

    /// <summary>
    /// Life advice and tips.
    /// 
    /// sa. Practicing everyday mindfulness can improve your memory and concentration skills....
    /// </summary>
    [Display(Name = "Life Advice", Description = "sa. Practicing everyday mindfulness can improve your memory and concentration skills...")]
    LifeAdvice = 1 << 1, // 2

    /// <summary>
    /// Fitness motivation.
    /// 
    /// sa. Never give up!
    /// </summary>
    [Display(Name = "Fitness Motivation", Description = "sa. Never give up!")]
    FitnessMotivation = 1 << 2, // 4

    /// <summary>
    /// Life motivation.
    /// 
    /// sa. Never give up!
    /// </summary>
    [Display(Name = "Life Motivation", Description = "sa. Never give up!")]
    LifeMotivation = 1 << 3, // 8

    /// <summary>
    /// Fitness affmirmations. 
    /// 
    /// sa. I'm getting stronger after every workout
    /// </summary>
    [Display(Name = "Fitness Affirmations", Description = "sa. I'm getting stronger after every workout.")]
    FitnessAffirmations = 1 << 4, // 16

    /// <summary>
    /// Life affirmations. 
    /// 
    /// sa. I'm a thoughtful and interesting person.
    /// </summary>
    [Display(Name = "Life Affirmations", Description = "sa. I'm a thoughtful and interesting person.")]
    LifeAffirmations = 1 << 5, // 32

    /// <summary>
    /// Mindfulness
    /// 
    /// sa. Breathe deeply. You are in the present moment.
    /// </summary>
    [Display(Name = "Mindfulness", Description = "sa. Breathe deeply. You are in the present moment.")]
    Mindfulness = 1 << 6, // 64

    /// <summary>
    /// Good vibes. Re-parenting.
    /// 
    /// sa. You are beautiful!
    /// </summary>
    [Display(Name = "Good Vibes", Description = "sa. You are beautiful!")]
    GoodVibes = 1 << 7, // 128

    /// <summary>
    /// Footnotes to show above the workout.
    /// </summary>
    Top = All & ~Bottom, // FitnessAffirmations | LifeAffirmations,

    /// <summary>
    /// Footnotes to show below the workout.
    /// </summary>
    Bottom = FitnessAdvice | FitnessMotivation | LifeAdvice | LifeMotivation | Mindfulness,

    All = FitnessAdvice | FitnessMotivation | FitnessAffirmations | LifeAdvice | LifeMotivation | LifeAffirmations | GoodVibes | Mindfulness
}
