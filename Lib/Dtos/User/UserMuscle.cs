using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lib.Dtos.User;

[Table("user_muscle")]
public class UserMuscle
{
    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Dtos.User.User.Id))]
    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserMuscles))]
    public virtual User User { get; init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    [NotMapped]
    public Range Range => new(Start, End);
}
