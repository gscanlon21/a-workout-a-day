using Core.Code.Extensions;
using Core.Dtos.User;
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

    public UserOptions(UserDto user, Section? section)
    {
        NoUser = false;
        Id = user.Id;
        Equipment = user.Equipment;
        CreatedDate = user.CreatedDate;
        IsNewToFitness = user.IsNewToFitness;

        // Don't filter out Rehab exercises when the section is unset or is the rehab section.
        if (section.HasValue && section != Section.None && !section.Value.HasAnyFlag32(Section.Rehab))
        {
            ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
        }
    }

    public UserOptions(Entities.User.User user, Section? section)
    {
        NoUser = false;
        Id = user.Id;
        Equipment = user.Equipment;
        CreatedDate = user.CreatedDate;
        IsNewToFitness = user.IsNewToFitness;

        // Don't filter out Rehab exercises when the section is unset or is the rehab section.
        if (section.HasValue && section != Section.None && !section.Value.HasAnyFlag32(Section.Rehab))
        {
            ExcludeRecoveryMuscle = user.RehabFocus.As<MuscleGroups>();
        }
    }
}
