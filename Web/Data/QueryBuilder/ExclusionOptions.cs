using Web.Entities.Exercise;
using Web.Models.User;

namespace Web.Data.QueryBuilder;

public class ExclusionOptions
{
    public ExclusionOptions() { }

    public ExclusionOptions(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            Exercises = exercises;
        }
    }

    public IEnumerable<int> ExerciseIds => Exercises.Select(e => e.Id);
    public IEnumerable<int> VariationIds => Variations.Select(e => e.Id);

    public IEnumerable<Exercise> Exercises { private get; set; } = new List<Exercise>();
    public IEnumerable<Variation> Variations { private get; set; } = new List<Variation>();
}
