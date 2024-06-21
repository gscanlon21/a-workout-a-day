using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;


/// <summary>
/// User's progression level of an exercise.
/// 
/// TODO Scopes.
/// TODO Single-use tokens.
/// </summary>
[Table("user_variation_weight")]
public class UserVariationWeightDto
{
    public UserVariationWeightDto() { }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [JsonIgnore]
    public virtual UserVariationDto UserVariation { get; init; } = null!;
}
