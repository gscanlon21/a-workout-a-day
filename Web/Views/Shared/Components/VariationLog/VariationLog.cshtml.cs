using Core.Code.Helpers;
using Data.Entities.User;

namespace Web.Views.Shared.Components.VariationLog;


public class VariationLogViewModel
{
    public VariationLogViewModel(IList<UserVariationWeight>? userWeights, UserVariation? current)
    {
        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            Xys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = DateHelpers.Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Weight);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(DateHelpers.Today, current.Weight)).ToList();
        }

        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            SetXys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = DateHelpers.Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Sets);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(DateHelpers.Today, current.Sets)).ToList();
        }

        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            RepXys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = DateHelpers.Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Reps);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(DateHelpers.Today, current.Reps)).ToList();
        }

        if (userWeights != null && current != null)
        {
            // Skip today, start at 1, because we append the current weight onto the end regardless.
            SecXys = Enumerable.Range(1, 365).Select(i =>
            {
                var date = DateHelpers.Today.AddDays(-i);
                return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Secs);
            }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(DateHelpers.Today, current.Secs)).ToList();
        }
    }

    public required bool IsWeighted { get; init; }

    internal IList<Xy> Xys { get; init; } = [];

    internal IList<Xy> RepXys { get; init; } = [];

    internal IList<Xy> SecXys { get; init; } = [];

    internal IList<Xy> SetXys { get; init; } = [];

    /// <summary>
    /// For chart.js
    /// </summary>
    internal record Xy(string X, int? Y)
    {
        internal Xy(DateOnly x, int? y) : this(x.ToString("O"), y) { }
    }
}
