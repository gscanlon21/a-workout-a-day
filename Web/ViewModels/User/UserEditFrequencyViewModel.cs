using System.ComponentModel.DataAnnotations.Schema;
using Web.Code.Extensions;
using Web.Entities.Newsletter;
using Web.Entities.User;
using Web.Models.Exercise;

namespace Web.ViewModels.User;

public class UserEditFrequencyViewModel
{
    public UserEditFrequencyViewModel()
    {
        Hide = true;
    }

    public UserEditFrequencyViewModel(NewsletterRotation rotation)
    {
        Day = rotation.Id;
        MuscleGroups = rotation.MuscleGroups;
        MovementPatterns = rotation.MovementPatterns;
        Hide = false;
    }

    public UserEditFrequencyViewModel(UserFrequency frequency)
    {
        Day = frequency.Rotation.Id;
        MuscleGroups = frequency.Rotation.MuscleGroups;
        MovementPatterns = frequency.Rotation.MovementPatterns;
        Hide = false;
    }

    public bool Hide { get; set; }

    public int Day { get; init; }

    public MuscleGroups MuscleGroups { get; set; }

    public MovementPattern MovementPatterns { get; set; }

    [NotMapped]
    public MuscleGroups[]? MuscleGroupsBinder
    {
        get => Enum.GetValues<MuscleGroups>().Where(e => MuscleGroups.HasFlag(e)).ToArray();
        set => MuscleGroups = value?.Aggregate(MuscleGroups.None, (a, e) => a | e) ?? MuscleGroups.None;
    }

    [NotMapped]
    public MovementPattern[]? MovementPatternsBinder
    {
        get => Enum.GetValues<MovementPattern>().Where(e => MovementPatterns.HasAnyFlag32(e)).ToArray();
        set => MovementPatterns = value?.Aggregate(MovementPattern.None, (a, e) => a | e) ?? MovementPattern.None;
    }
}
