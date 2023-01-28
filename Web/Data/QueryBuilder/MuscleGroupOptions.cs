using System.Linq.Expressions;
using System.Numerics;
using Web.Entities.Exercise;
using Web.Models.Exercise;

namespace Web.Data.QueryBuilder;

public class MuscleGroupOptions
{
    private int? _atLeastXUniqueMusclesPerExercise;

    public MuscleGroupOptions() { }

    public MuscleGroupOptions(MuscleGroups muscleGroups)
    {
        if (muscleGroups == MuscleGroups.None)
        {
            throw new ArgumentOutOfRangeException(nameof(MuscleGroups));
        }

        MuscleGroups = muscleGroups;
    }

    public Expression<Func<IExerciseVariationCombo, MuscleGroups>> MuscleTarget { get; set; } = v => v.Variation.StrengthMuscles;

    public MuscleGroups MuscleGroups { get; } = MuscleGroups.All;

    /// <summary>
    ///     If null, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If MuscleGroups.None, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If > MuscleGroups.None, excludes these muscle groups from the IncludeMuscle or MuscleGroups set.
    /// </summary>
    public MuscleGroups? ExcludeMuscleGroups { get; set; }

    /// <summary>
    ///     If null, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If MuscleGroups.None, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If > MuscleGroups.None, excludes these muscle groups from the IncludeMuscle or MuscleGroups set.
    /// </summary>
    public MuscleGroups? ExcludeRecoveryMuscle { get; set; }

    /// <summary>
    ///     Makes sure each variations works at least x unique muscle groups to be choosen.
    ///     
    ///     If no variations can be found, will drop x by 1 and look again until all muscle groups are accounted for.
    /// </summary>
    public int? AtLeastXUniqueMusclesPerExercise
    {
        get => _atLeastXUniqueMusclesPerExercise;
        set
        {
            if (value > BitOperations.PopCount((ulong)MuscleGroups))
            {
                throw new ArgumentOutOfRangeException(nameof(AtLeastXUniqueMusclesPerExercise));
            }

            _atLeastXUniqueMusclesPerExercise = value;
        }
    }
}