namespace Web.Data.Query.Options;

public class WeightOptions
{
    public WeightOptions() { }

    public WeightOptions(bool? onlyWeights)
    {
        OnlyWeights = onlyWeights;
    }

    /// <summary>
    ///     If true, prefer weighted variations over bodyweight variations.
    ///     If false, only show bodyweight variations.
    ///     If null, show both weighted and bodyweight variations with equal precedence.
    /// </summary>
    public bool? OnlyWeights { get; private set; } = null;
}
