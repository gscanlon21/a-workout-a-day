using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class MuscleContractionsOptions : IOptions
{
    public MuscleContractionsOptions() { }

    public MuscleContractionsOptions(MuscleContractions? muscleContractions)
    {
        MuscleContractions = muscleContractions;
    }

    public MuscleContractions? MuscleContractions { get; set; }
}
