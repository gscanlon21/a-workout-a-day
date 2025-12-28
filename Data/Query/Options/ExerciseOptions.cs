using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.Newsletter;
using Data.Entities.Users;

namespace Data.Query.Options;

public class ExerciseOptions : IOptions
{
    private readonly Section _section;

    public ExerciseOptions() { }

    public ExerciseOptions(Section section)
    {
        _section = section;
    }

    /// <summary>
    /// Only select these exercises.
    /// </summary>
    public List<int>? ExerciseIds { get; private set; }

    /// <summary>
    /// Only select these variations.
    /// </summary>
    public List<int>? VariationIds { get; private set; }

    public void AddPastVariations(ICollection<UserWorkoutVariation> userWorkoutVariations)
    {
        VariationIds = userWorkoutVariations
            .Where(nv => _section == nv.Section)
            .Select(nv => nv.VariationId)
            .ToList();
    }

    /// <summary>
    /// Only select these exercises.
    /// </summary>
    public void AddExercises(IEnumerable<UserExercise>? exercises)
    {
        if (exercises != null)
        {
            if (ExerciseIds == null)
            {
                ExerciseIds = exercises.Select(e => e.ExerciseId).ToList();
            }
            else
            {
                ExerciseIds.AddRange(exercises.Select(e => e.ExerciseId));
            }
        }
    }

    /// <summary>
    /// Only select these exercises.
    /// </summary>
    public void AddExercisePrerequisites(IEnumerable<ExercisePrerequisite>? exercises)
    {
        if (exercises != null)
        {
            if (ExerciseIds == null)
            {
                ExerciseIds = exercises.Select(e => e.PrerequisiteExerciseId).ToList();
            }
            else
            {
                ExerciseIds.AddRange(exercises.Select(e => e.PrerequisiteExerciseId));
            }
        }
    }

    /// <summary>
    /// Only select these exercises.
    /// </summary>
    public void AddExercisePostrequisites(IEnumerable<ExercisePrerequisite>? exercises)
    {
        if (exercises != null)
        {
            if (ExerciseIds == null)
            {
                ExerciseIds = exercises.Select(e => e.ExerciseId).ToList();
            }
            else
            {
                ExerciseIds.AddRange(exercises.Select(e => e.ExerciseId));
            }
        }
    }

    /// <summary>
    /// Only select these variations.
    /// </summary>
    public void AddVariations(IEnumerable<UserVariation>? variations)
    {
        if (variations != null)
        {
            if (VariationIds == null)
            {
                VariationIds = variations.Select(e => e.VariationId).ToList();
            }
            else
            {
                VariationIds.AddRange(variations.Select(e => e.VariationId));
            }
        }
    }

    public bool HasData() => true;
}
