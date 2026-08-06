using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Users;

[Table("user_prehab_skill")]
public class UserPrehabSkill
{
    [ForeignKey(nameof(Users.User.Id))]
    public int UserId { get; init; }

    public int Skills { get; set; }

    public PrehabFocus PrehabFocus { get; init; }

    [Range(UserConsts.PrehabCountMin, UserConsts.PrehabCountMax)]
    public int Count { get; set; } = UserConsts.PrehabCountDefault;


    #region Navigation Properties

    [JsonIgnore, InverseProperty(nameof(Users.User.UserPrehabSkills))]
    public virtual User User { get; private init; } = null!;

    #endregion
}
