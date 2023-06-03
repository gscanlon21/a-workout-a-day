using System.ComponentModel.DataAnnotations.Schema;
using Web.Models.Exercise;

namespace Web.Entities.User;

[Table("user_muscle")]
public class UserMuscle
{
    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [InverseProperty(nameof(Entities.User.User.UserMuscles))]
    public virtual User User { get; private init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    [NotMapped]
    public Range Range => new (Start, End);
}
