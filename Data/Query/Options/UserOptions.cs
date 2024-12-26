using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.User;
using System.Linq.Expressions;

namespace Data.Query.Options;

public class UserOptions : IOptions
{
    public bool NoUser { get; } = true;

    public int Id { get; }
    public Equipment Equipment { get; }
    public Intensity Intensity { get; }
    public bool IsNewToFitness { get; }
    public DateOnly CreatedDate { get; }

    public bool IgnoreIgnored { get; set; } = false;
    public bool IgnoreProgressions { get; set; } = false;
    public bool IgnorePrerequisites { get; set; } = false;
    public bool IgnoreMissingEquipment { get; set; } = false;

    /// <summary>
    ///     If null, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If MuscleGroups.None, does not exclude any muscle groups from the IncludeMuscle or MuscleGroups set.
    ///     If > MuscleGroups.None, excludes these muscle groups from the IncludeMuscle or MuscleGroups set.
    /// </summary>
    public MusculoskeletalSystem? ExcludeRecoveryMuscle { get; }

    /// <summary>
    /// This says what (strengthening/secondary/stretching) muscles we should abide by when excluding variations for ExcludeRecoveryMuscle.
    /// </summary>
    public Expression<Func<IExerciseVariationCombo, MusculoskeletalSystem>> ExcludeRecoveryMuscleTarget { get; } = v => v.Variation.Strengthens;


    public UserOptions() { }

    public UserOptions(User user, Section? section)
    {
        Id = user.Id;
        NoUser = false;
        Equipment = user.Equipment;
        Intensity = user.Intensity;
        CreatedDate = user.CreatedDate;
        IsNewToFitness = user.IsNewToFitness;

        // Don't filter out recovery exercises when the section is unset or if its the rehab section.
        if (section.HasValue && section != Section.None && !section.Value.HasAnyFlag(Section.Rehab))
        {
            // Don't filter out recovery exercises when the injured muscle group is not a part of our normal strengthening routine.
            var strengtheningMuscleGroups = user.UserMuscleStrengths.NullIfEmpty()?.Select(s => s.MuscleGroup) ?? UserMuscleStrength.MuscleTargets.Keys;
            ExcludeRecoveryMuscle = user.RehabFocus.As<MusculoskeletalSystem>() & strengtheningMuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr, n) => curr | n);
        }
    }

    public bool HasData() => true;
}
