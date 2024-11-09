using Core.Consts;
using Data.Entities.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

/// <summary>
/// User's set/rep/sec/weight tracking history of an exercise.
/// </summary>
[Table("user_variation_log")]
public class UserVariationLog
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public int UserVariationId { get; set; }

    [Range(UserConsts.UserWeightMin, UserConsts.UserWeightMax)]
    public int Weight { get; set; } = UserConsts.UserWeightDefault;

    [Range(UserConsts.UserSetsMin, UserConsts.UserSetsMax)]
    public int Sets { get; set; } = UserConsts.UserSetsDefault;

    [Range(UserConsts.UserRepsMin, UserConsts.UserRepsMax)]
    public int Reps { get; set; } = UserConsts.UserRepsDefault;

    [Range(UserConsts.UserSecsMin, UserConsts.UserSecsMax)]
    public int Secs { get; set; } = UserConsts.UserSecsDefault;

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Date { get; init; } = DateHelpers.Today;

    [JsonIgnore, InverseProperty(nameof(Entities.User.UserVariation.UserVariationLogs))]
    public virtual UserVariation UserVariation { get; private init; } = null!;

    public Proficiency? GetProficiency()
    {
        if (Sets > 0 && (Reps > 0 || Secs > 0))
        {
            return new Proficiency(Secs, Reps) { Sets = Sets };
        }

        return null;
    }

    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is UserVariationLog other
        && other.Id == Id;
}