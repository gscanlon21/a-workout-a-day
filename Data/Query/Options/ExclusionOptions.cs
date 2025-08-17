using Core.Models.Exercise.Skills;
using Data.Entities.Exercise;

namespace Data.Query.Options;

// I should add an option to exclude on similarity scores.
public class ExclusionOptions : IOptions
{
    /// <summary>
    /// Will not choose any exercises that fall in this list.
    /// </summary>
    public List<int> ExerciseIds = [];

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public List<int> VariationIds = [];

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public VocalSkills VocalSkills;

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public VisualSkills VisualSkills;

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public CervicalSkills CervicalSkills;

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public ThoracicSkills ThoracicSkills;

    /// <summary>
    /// Will not choose any variations that fall in this list.
    /// </summary>
    public LumbarSkills LumbarSkills;

    /// <summary>
    /// Exclude any variation of these exercises from being chosen.
    /// </summary>
    public void AddExcludeExercises(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            ExerciseIds.AddRange(exercises.Select(e => e.Id));
        }
    }

    /// <summary>
    /// Exclude any of these variations from being chosen.
    /// </summary>
    public void AddExcludeVariations(IEnumerable<Variation>? variations)
    {
        if (variations != null)
        {
            VariationIds.AddRange(variations.Select(e => e.Id));
        }
    }

    /// <summary>
    /// Exclude any variations from being chosen that are a part of these exercise groups.
    /// </summary>
    public void AddExcludeSkills(IEnumerable<Exercise>? exercises)
    {
        if (exercises != null)
        {
            foreach (var exercise in exercises)
            {
                VocalSkills |= exercise.VocalSkills;
                VisualSkills |= exercise.VisualSkills;
                CervicalSkills |= exercise.CervicalSkills;
                ThoracicSkills |= exercise.ThoracicSkills;
                LumbarSkills |= exercise.LumbarSkills;
            }
        }
    }

    public bool HasData() => true;
}
