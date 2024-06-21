using Data.Entities.User;
using System.Linq;

namespace Web.Views.Shared.Components.VariationLog;


public class VariationLogViewModel
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public VariationLogViewModel(IList<UserVariationWeight>? userWeights, Data.Entities.User.UserVariation? current)
    {
        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            Xys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Weight);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(Today, current.Weight)).ToList();
        }

        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            SetXys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Sets);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(Today, current.Sets)).ToList();
        }

        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            RepXys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Reps);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(Today, current.Reps)).ToList();
        }
    }

    public required bool IsWeighted { get; init; }

    internal IList<Xy> Xys { get; init; } = [];

    internal IList<Xy> RepXys { get; init; } = [];

    internal IList<Xy> SetXys { get; init; } = [];

    /// <summary>
    /// For chart.js
    /// </summary>
    internal record Xy(string X, int? Y)
    {
        internal Xy(DateOnly x, int? y) : this(x.ToString("O"), y) { }
    }
}
