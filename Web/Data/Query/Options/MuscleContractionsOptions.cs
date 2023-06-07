using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class MuscleContractionsOptions : IOptions
{
    public MuscleContractionsOptions() { }

    public MuscleContractionsOptions(MuscleContractions? muscleContractions)
    {
        MuscleContractions = muscleContractions;
    }

    public MuscleContractions? MuscleContractions { get; set; }
}
