using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_prehab_skill")]
public class UserPrehabSkill
{
    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserPrehabSkills))]
    public virtual User User { get; private init; } = null!;

    public PrehabFocus PrehabFocus { get; init; }

    /// <summary>
    /// Should this prehab focus only select variations that are due for refresh?
    /// Skips variations that have refresh padding, regardless of available variations.
    /// </summary>
    public bool AllRefreshed { get; set; }

    [Range(UserConsts.PrehabCountMin, UserConsts.PrehabCountMax)]
    public int Count { get; set; } = UserConsts.PrehabCountDefault;

    public int Skills { get; set; }

    /// <summary>
    /// Cap the max number of exercises to the max listed Skill types.
    /// </summary>
    public int? SkillCount => AllRefreshed ? Count : Math.Clamp(BitOperations.PopCount((ulong)Skills), UserConsts.PrehabCountMin, Count);
}
