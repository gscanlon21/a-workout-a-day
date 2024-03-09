using Core.Code.Extensions;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using System.Linq.Expressions;

namespace Data.Query.Options;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;

    public int Id { get; }
    public Equipment Equipment { get; }
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }
    public int RefreshExercisesAfterXWeeks { get; }

    /// <summary>
    /// Defaults to 1 for no change when there is no user.
    /// </summary>
    public double WeightCoreExercisesXTimesMore { get; } = 1;

    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;
    public bool IgnoreIgnored { get; set; } = false;
    public bool IgnoreMissingEquipment { get; set; } = false;

    /// <summary>
    ///     If null, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If MuscleGroups.None, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If > MuscleGroups.None, excludes these muscle groups from the IncludeMuscle or MuscleGroups set.
    /// </summary>
    public MuscleGroups? ExcludeRecoveryMuscle { get; }

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when excluding variations for ExcludeRecoveryMuscle.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MuscleGroups>> ExcludeRecoveryMuscleTarget { get; } = v => v.Variation.StrengthMuscles;


    public UserOptions() { }

    public UserOptions(Entities.User.User user, Section? section)
    {
        NoUser = false;
        Id = user.Id;
        Equipment = user.Equipment;
        CreatedDate = user.CreatedDate;
        IsNewToFitness = user.IsNewToFitness;
        ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
        WeightCoreExercisesXTimesMore = user.WeightCoreExercisesXTimesMore;

        RefreshExercisesAfterXWeeks = section switch
        {
            Section.Functional => user.RefreshFunctionalEveryXWeeks,
            Section.Accessory => user.RefreshAccessoryEveryXWeeks,
            _ => 0
        };
    }
}
