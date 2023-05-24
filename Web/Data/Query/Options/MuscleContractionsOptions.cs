using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class MuscleContractionsOptions
{
    public MuscleContractionsOptions() { }

    public MuscleContractionsOptions(MuscleContractions? muscleContractions)
    {
        MuscleContractions = muscleContractions;
    }

    public MuscleContractions? MuscleContractions { get; set; }
}
