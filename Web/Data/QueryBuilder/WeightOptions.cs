namespace Web.Data.QueryBuilder;

public class WeightOptions
{
    public WeightOptions() { }

    public WeightOptions(bool? prefersWeights)
    {
        if (PrefersWeights == true)
        {
            throw new ArgumentNullException(nameof(PrefersWeights), "Prefers weights cannot be true.");
        }

        PrefersWeights = prefersWeights;
    }

    /// <summary>
    ///     If true, prefer weighted variations over bodyweight variations.
    ///     If false, only show bodyweight variations.
    ///     If null, show both weighted and bodyweight variations with equal precedence.
    /// </summary>
    public bool? PrefersWeights { get; private set; } = null;

    public bool OnlyWeights { get; set; }
}
