using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Query.Options;

namespace Data.Query.Builders.MuscleGroup;

public interface IMuscleGroupBuilder
{
    MuscleGroupOptions Build(Section section);
}

/// <summary>
/// Step-builder pattern for muscle target options.
/// </summary>
public class MuscleGroupBuilder : IMuscleGroupBuilder
{
    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    private IList<MusculoskeletalSystem> MuscleGroups;

    private MuscleGroupBuilder(IList<MusculoskeletalSystem> muscleGroups)
    {
        MuscleGroups = muscleGroups;
    }

    public static IMuscleGroupBuilder WithMuscleGroups(IList<MusculoskeletalSystem> muscleGroups)
    {
        return new MuscleGroupBuilder(muscleGroups);
    }

    public MuscleGroupOptions Build(Section section)
    {
        return new MuscleGroupOptions(MuscleGroups, [], []);
    }
}