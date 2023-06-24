using Core.Models.Exercise;


namespace Core.Dtos.User;

public interface IUserMuscle
{
    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    //[JsonIgnore, InverseProperty(nameof(Dtos.User.User.UserMuscles))]
    public IUser User { get; init; }

    public int Start { get; set; }

    public int End { get; set; }

    public Range Range => new(Start, End);
}
