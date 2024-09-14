using Core.Consts;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Data.Query.Options;

public class MuscleGroupOptions : IOptions
{
    private int? _atLeastXMusclesPerExercise;
    private int? _atLeastXUniqueMusclesPerExercise;

    public MuscleGroupOptions() { }

    public MuscleGroupOptions(IList<MusculoskeletalSystem> muscleGroups, IDictionary<MusculoskeletalSystem, int> muscleTargetsRDA, IDictionary<MusculoskeletalSystem, int> muscleTargetsTUL)
    {
        MuscleGroups = muscleGroups;
        MuscleTargetsRDA = muscleTargetsRDA;
        MuscleTargetsTUL = muscleTargetsTUL;
        AllMuscleGroups = muscleGroups.Aggregate(Core.Models.Exercise.MusculoskeletalSystem.None, (curr, n) => curr | n);
    }

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// This contains only the muscle groups that we want to target.
    /// </summary>
    public MusculoskeletalSystem AllMuscleGroups { get; }

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// This contains only the muscle groups that we want to target.
    /// </summary>
    public IList<MusculoskeletalSystem> MuscleGroups { get; } = [];

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// This contains all of the muscle groups that we are tracking.
    /// </summary>
    public IDictionary<MusculoskeletalSystem, int> MuscleTargetsRDA { get; } = new Dictionary<MusculoskeletalSystem, int>();

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// This contains all of the muscle groups that we are tracking.
    /// </summary>
    public IDictionary<MusculoskeletalSystem, int> MuscleTargetsTUL { get; } = new Dictionary<MusculoskeletalSystem, int>();

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when selecting variations.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MusculoskeletalSystem>> MuscleTarget { get; set; } = v => v.Variation.Strengthens;

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when selecting variations.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MusculoskeletalSystem>>? SecondaryMuscleTarget { get; set; }

    /// <summary>
    ///     Makes sure each variations works at least x unique muscle groups to be chosen.
    ///     
    ///     If no variations can be found, will drop x by 1 and look again until all muscle groups are accounted for.
    /// </summary>
    [Range(ExerciseConsts.AtLeastXUniqueMusclesPerExerciseMin, ExerciseConsts.AtLeastXUniqueMusclesPerExerciseMax)]
    public int? AtLeastXUniqueMusclesPerExercise
    {
        get => _atLeastXUniqueMusclesPerExercise;
        set => _atLeastXUniqueMusclesPerExercise = value;
    }

    /// <summary>
    /// Minimum value for AtLeastXUniqueMusclesPerExercise.
    /// </summary>
    [Range(ExerciseConsts.AtLeastXMusclesPerExerciseMin, ExerciseConsts.AtLeastXMusclesPerExerciseMax)]
    public int? AtLeastXMusclesPerExercise
    {
        get => _atLeastXMusclesPerExercise;
        set => _atLeastXMusclesPerExercise = value;
    }

    public bool HasData() => true;
}