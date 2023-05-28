namespace Web.Models.Footnote;

[Flags]
public enum FootnoteType
{
    /// <summary>
    /// Take five to 10 minutes to warm up and cool down properly.
    /// </summary>
    FitnessAdvice = 1 << 0, // 1
    
    /// <summary>
    /// You are beautiful!
    /// </summary>
    Inspirations = 1 << 1, // 2
    
    /// <summary>
    /// I'm a thoughtful and interesting person.
    /// </summary>
    Affirmations = 1 << 2, // 4

    All = FitnessAdvice | Inspirations | Affirmations
}
