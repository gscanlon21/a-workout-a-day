using System.Linq.Expressions;
using Web.Models.Exercise;

namespace Web.Data.Query.Options;

public class MuscleGroupOptions
{
    private int? _atLeastXMusclesPerExercise;
    private int? _atLeastXUniqueMusclesPerExercise;

    public MuscleGroupOptions() { }

    public MuscleGroupOptions(MuscleGroups muscleGroups)
    {
        MuscleGroups = muscleGroups;
    }

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when selecting variations.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MuscleGroups>> MuscleTarget { get; set; } = v => v.Variation.StrengthMuscles;

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when selecting variations.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MuscleGroups>>? SecondaryMuscleTarget { get; set; }

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public MuscleGroups MuscleGroups { get; } = MuscleGroups.None;

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public IDictionary<MuscleGroups, int> MuscleTargets { get; set; } = new Dictionary<MuscleGroups, int>();

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
            //if (value > BitOperations.PopCount((ulong)MuscleGroups))
            //{
            //throw new ArgumentOutOfRangeException(nameof(AtLeastXUniqueMusclesPerExercise));
            //}

            _atLeastXUniqueMusclesPerExercise = value;
        }
    }

    public int? AtLeastXMusclesPerExercise
    {
        get => _atLeastXMusclesPerExercise;
        set
        {
            //if (value > BitOperations.PopCount((ulong)MuscleGroups))
            //{
            //    throw new ArgumentOutOfRangeException(nameof(AtLeastXMusclesPerExercise));
            //}

            _atLeastXMusclesPerExercise = value;
        }
    }
}