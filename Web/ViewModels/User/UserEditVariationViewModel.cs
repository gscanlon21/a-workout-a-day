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

    public UserManageVariationViewModel(IList<UserVariationWeight> userWeights)
    {
        Xys = Enumerable.Range(0, 365).Select(i =>
        {
            var date = Today.AddDays(-i);
            return new Xy(date)
            {
                Y = userWeights.FirstOrDefault(uq => uq.Date == date)?.Weight
            };
        }).OrderBy(xy => xy.X).SkipWhile(xy => !xy.Y.HasValue).ToList();
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
    internal class Xy
    {
        public Xy(DateOnly x)
        {
            X = x.ToString("O");
        }

        public Xy(UserVariationWeight userWeight) : this(userWeight.Date)
        {
            Y = userWeight.Weight;
        }

        public string X { get; set; }
        public int? Y { get; set; }
    }
}
