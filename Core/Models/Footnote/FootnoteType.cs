using System.ComponentModel.DataAnnotations;

namespace Core.Models.Footnote;

[Flags]
public enum FootnoteType
{
    [Display(Name = "None")]
    None = 0,

    /// <summary>
    /// Fitness advice and tips. 
    /// 
    /// sa. Take five to 10 minutes to warm up and cool down properly.
    /// </summary>
    [Display(Name = "Fitness Tips")]
    FitnessTips = 1 << 0, // 1

    /// <summary>
    /// Life advice and tips.
    /// 
    /// sa. Practicing everyday mindfulness can improve your memory and concentration skills....
    /// </summary>
    [Display(Name = "Health Tips")]
    HealthTips = 1 << 1, // 2

    /// <summary>
    /// User defined footnotes.
    /// </summary>
    [Display(Name = "Fitness Facts")]
    FitnessFacts = 1 << 2, // 4

    /// <summary>
    /// User defined footnotes.
    /// </summary>
    [Display(Name = "Health Facts")]
    HealthFacts = 1 << 3, // 8

    /// <summary>
    /// Fitness motivation.
    /// 
    /// sa. Never give up!
    /// </summary>
    [Display(Name = "Fitness Motivation")]
    FitnessMotivation = 1 << 4, // 16

    /// <summary>
    /// Fitness affirmations. 
    /// 
    /// sa. I'm getting stronger after every workout
    /// </summary>
    [Display(Name = "Fitness Affirmations")]
    FitnessAffirmations = 1 << 5, // 32

    /// <summary>
    /// Mindfulness
    /// 
    /// sa. Breathe deeply. You are in the present moment.
    /// </summary>
    [Display(Name = "Mindfulness")]
    Mindfulness = 1 << 6, // 64

    /// <summary>
    /// Good vibes. Re-parenting.
    /// 
    /// sa. You are beautiful!
    /// </summary>
    [Display(Name = "Good Vibes")]
    GoodVibes = 1 << 7, // 128

    /// <summary>
    /// User defined footnotes.
    /// </summary>
    [Display(Name = "Custom")]
    Custom = 1 << 8, // 256

    System = All & ~Custom, // FitnessTips | FitnessFacts | FitnessMotivation | FitnessAffirmations | HealthTips | HealthFacts | GoodVibes | Mindfulness

    All = FitnessTips | FitnessFacts | FitnessMotivation | FitnessAffirmations | HealthTips | HealthFacts | GoodVibes | Mindfulness | Custom
}
