using Core.Models.Exercise;
using Data.Entities.Exercise;

namespace Data.Data.Query.Options;

public class ExclusionOptions : IOptions
{
    /// <summary>
    /// Will not choose any exercises that fall in this list.
    /// </summary>
    public List<int> ExerciseIds = new();

    /// <summary>
    /// Will not choose any exercises that fall in this list.
    /// </summary>
    public List<int> ExerciseVariationIds = new();

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public List<int> VariationIds = new();

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public ExerciseGroup ExerciseGroups = ExerciseGroup.None;

    /// <summary>
    /// Exclude any variation of these exercises from being choosen.
    /// </summary>
    public void AddExcludeExercises(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            ExerciseIds.AddRange(exercises.Select(e => e.Id));
        }
    }

    /// <summary>
    /// Exclude any variation of these exercises from being choosen.
    /// </summary>
    public void AddExcludeExerciseVariations(IEnumerable<ExerciseVariation>? exerciseVariations)
    {
        if (exerciseVariations != null)
        {
            ExerciseVariationIds.AddRange(exerciseVariations.Select(e => e.Id));
        }
    }

    /// <summary>
    /// Exclude any of these variations from being choosen.
    /// </summary>
    public void AddExcludeVariations(IEnumerable<Variation>? variations)
    {
        if (variations != null)
        {
            VariationIds.AddRange(variations.Select(e => e.Id));
        }
    }

    /// <summary>
    /// Exclude any variations from being choosen that are a part of these exercise groups.
    /// </summary>
    public void AddExcludeGroups(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            ExerciseGroups = exercises.Aggregate(ExerciseGroups, (c, n) => c | n.Groups);
        }
    }

    /// <summary>
    /// Exclude any variations from being choosen that are a part of these exercise groups.
    /// </summary>
    public void AddExcludeGroups(ExerciseGroup exerciseGroups)
    {
        ExerciseGroups |= exerciseGroups;
    }
}
