using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class MuscleMovementOptions
{
    public MuscleMovementOptions() { }

    public MuscleMovementOptions(MuscleMovement? muscleMovement)
    {
        MuscleMovement = muscleMovement;
    }

    public MuscleMovement? MuscleMovement { get; set; }
}
