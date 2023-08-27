using Core.Models.Exercise;
using Data.Entities.Newsletter;
using Data.Entities.User;
using System.ComponentModel.DataAnnotations;

namespace Web.ViewModels.User;

public class UserEditFrequencyViewModel : IValidatableObject
{
    public UserEditFrequencyViewModel()
    {
        Hide = true;
    }

    public UserEditFrequencyViewModel(WorkoutRotation rotation)
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

    public MovementPattern MovementPatterns { get; set; }

    public IList<MuscleGroups>? MuscleGroups { get; set; }

    public MovementPattern[]? MovementPatternsBinder
    {
        get => Enum.GetValues<MovementPattern>().Where(e => MovementPatterns.HasFlag(e)).ToArray();
        set => MovementPatterns = value?.Aggregate(MovementPattern.None, (a, e) => a | e) ?? MovementPattern.None;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Hide)
        {
            if (MovementPatterns == MovementPattern.None && MuscleGroups?.Any() != true)
            {
                yield return new ValidationResult("At least one movement pattern or muscle group is required.",
                    new List<string>() {
                        nameof(Day)
                    }
                );
            }
        }
    }
}
