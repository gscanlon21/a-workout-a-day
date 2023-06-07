using Web.Entities.Exercise;

namespace Web.Data.Query.Options;

public class ExerciseOptions : IOptions
{
    /// <summary>
    /// Will not choose any exercises that fall in this list.
    /// </summary>
    public List<int>? ExerciseIds { get; private set; }

    /// <summary>
    /// Will not choose any exercises that fall in this list.
    /// </summary>
    public List<int>? ExerciseVariationIds { get; set; }

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public List<int>? VariationIds { get; private set; }

    /// <summary>
    /// Exclude any variation of these exercises from being choosen.
    /// </summary>
    public void AddExercises(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            if (ExerciseIds == null)
            {
                ExerciseIds = exercises.Select(e => e.Id).ToList();
            }
            else
            {
                ExerciseIds.AddRange(exercises.Select(e => e.Id));
            }
        }
    }

    /// <summary>
    /// Exclude any variation of these exercises from being choosen.
    /// </summary>
    public void AddExerciseVariations(IEnumerable<ExerciseVariation>? exerciseVariationss)
    {
        if (exerciseVariationss != null)
        {
            if (ExerciseVariationIds == null)
            {
                ExerciseVariationIds = exerciseVariationss.Select(e => e.Id).ToList();
            }
            else
            {
                ExerciseVariationIds.AddRange(exerciseVariationss.Select(e => e.Id));
            }
        }
    }

    /// <summary>
    /// Exclude any of these variations from being choosen.
    /// </summary>
    public void AddVariations(IEnumerable<Variation>? variations)
    {
        if (variations != null)
        {
            if (VariationIds == null)
            {
                VariationIds = variations.Select(e => e.Id).ToList();
            }
            else
            {
                VariationIds.AddRange(variations.Select(e => e.Id));
            }
        }
    }
}
