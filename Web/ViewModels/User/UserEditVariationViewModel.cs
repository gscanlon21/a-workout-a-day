using Data.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserManageVariationViewModel
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    [Obsolete("Public parameterless constructor for model binding.", error: true)]
    public UserManageVariationViewModel() { }

    public UserManageVariationViewModel(IList<UserVariationWeight> userWeights, int currentWeight)
    {
        // Skip today, start at 1, because we append the current weight onto the end regardless.
        Xys = Enumerable.Range(1, 365).Select(i =>
        {
            var date = Today.AddDays(-i);
            return new Xy(date, userWeights.FirstOrDefault(uw => uw.Date == date)?.Weight);
        }).Where(xy => xy.Y.HasValue).Reverse().Append(new Xy(Today, currentWeight)).ToList();
    }

    public int VariationId { get; init; }

    /// <summary>
    /// If null, user has not yet tried to update.
    /// If true, user has successfully updated.
    /// If false, user failed to update.
    /// </summary>
    public bool? WasUpdated { get; set; }

    [Required]
    [Display(Name = "Email")]
    public string Email { get; init; } = null!;

    [Display(Name = "Variation")]
    public string? VariationName { get; set; } = null!;

    [Required]
    public string Token { get; init; } = null!;

    /// <summary>
    /// How often to take a deload week
    /// </summary>
    [Required, Range(0, 999)]
    [Display(Name = "How much weight are you able to lift?")]
    public int Weight { get; init; }

    internal IList<Xy> Xys { get; init; } = new List<Xy>();

    /// <summary>
    /// For chart.js
    /// </summary>
    internal record Xy(string X, int? Y)
    {
        internal Xy(DateOnly x, int? y) : this(x.ToString("O"), y) { }
    }
}
