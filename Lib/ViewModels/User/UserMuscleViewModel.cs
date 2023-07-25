using Core.Models.Exercise;
using System.Text.Json.Serialization;

namespace Lib.ViewModels.User;

public class UserMuscleViewModel
{
    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    [JsonInclude]
    public UserViewModel User { get; init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    public Range Range => new(Start, End);
}
