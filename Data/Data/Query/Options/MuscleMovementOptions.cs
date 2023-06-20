using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class MuscleMovementOptions : IOptions
{
    public MuscleMovementOptions() { }

    public MuscleMovementOptions(MuscleMovement? muscleMovement)
    {
        MuscleMovement = muscleMovement;
    }

    public MuscleMovement? MuscleMovement { get; set; }
}
