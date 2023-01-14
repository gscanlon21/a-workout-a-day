namespace Web.Code.Extensions;

public static class MathExtensions
{
    public static double Avg(params double[] values)
    {
        return values.Aggregate(0d, (acc, curr) => acc + curr) / values.Length;
    }

    public static int CeilToX(int x, double value)
    {
        return x * (int)Math.Ceiling(value / x);
    }

    public static int RoundToX(int x, double value)
    {
        return x * (int)Math.Round(value / x);
    }

    public static int FloorToX(int x, double value)
    {
        return x * (int)Math.Floor(value / x);
    }
}
