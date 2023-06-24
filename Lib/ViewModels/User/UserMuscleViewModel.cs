using Core.Models.Exercise;


namespace Lib.ViewModels.User;

public class UserMuscleViewModel
{
    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserMuscles))]
    public virtual UserViewModel User { get; init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    public Range Range => new(Start, End);
}
