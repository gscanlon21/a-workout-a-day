using Core.Dtos.Exercise;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// User's intensity stats.
/// </summary>
public interface IUserVariation
{
    [Required]
    public int UserId { get; init; }

    [Required]
    public int VariationId { get; init; }

    /// <summary>
    /// Don't show this variation to the user.
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    [Required]
    public int Pounds { get; set; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserVariations))]
    public IUser User { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserVariations))]
    public IVariation Variation { get; init; }
}
