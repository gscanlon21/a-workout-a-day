using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

/// <summary>
/// For CRUD actions
/// </summary>
public class UserEditVariationViewModel
{
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
    [Display(Name = "How much weight are you able to lift? (lbs)")]
    public int Pounds { get; init; }
}
