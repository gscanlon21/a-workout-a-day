using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_prehab_skill")]
public class UserPrehabSkill
{
    public PrehabFocus PrehabFocus { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserPrehabSkills))]
    public virtual User User { get; private init; } = null!;

    public bool AllRefreshed { get; set; }

    [Range(1, 9)]
    public int Count { get; set; } = 1;

    public int Skills { get; set; }

    /// <summary>
    /// Cap the max number of exercises to the max listed Skill types.
    /// </summary>
    public int? SkillCount => AllRefreshed ? int.MaxValue : Math.Max(Count, BitOperations.PopCount((ulong)Skills));
}
