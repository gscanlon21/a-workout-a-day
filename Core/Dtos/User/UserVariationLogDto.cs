using Core.Code.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.User;

/// <summary>
/// User's set/rep/sec/weight tracking history of an exercise.
/// </summary>
public class UserVariationLogDto
{
    public int Id { get; init; }

    public int Weight { get; set; }

    public int Sets { get; set; }

    public int Reps { get; set; }

    [Required]
    public int UserVariationId { get; set; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Date { get; init; } = DateHelpers.Today;
}
