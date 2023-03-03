namespace Web.Code.Extensions;

public static class MathExtensions
{
    /// <summary>
    /// Finds the mean of two or more numbers.
    /// </summary>
    public static double Avg(params double[] values)
    {
        return values.Aggregate(0d, (acc, curr) => acc + curr) / values.Length;
    }

    /// <summary>
    /// Rounds up to the next `x` step
    /// </summary>
    public static int CeilToX(int x, double value)
    {
        return x * (int)Math.Ceiling(value / x);
    }

    /// <summary>
    /// Rounds to the next `x` step
    /// </summary>
    public static int RoundToX(int x, double value)
    {
        return x * (int)Math.Round(value / x);
    }

    /// <summary>
    /// Rounds down to the next `x` step
    /// </summary>
    public static int FloorToX(int x, double value)
    {
        return x * (int)Math.Floor(value / x);
    }
}
