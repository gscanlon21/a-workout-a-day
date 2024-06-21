using Core.Consts;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

[Table("user_muscle_mobility")]
public class UserMuscleMobilityDto
{
    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;

    [Range(UserConsts.UserMuscleMobilityMin, UserConsts.UserMuscleMobilityMax)]
    public int Count { get; set; }

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, int> MuscleTargets = new Dictionary<MuscleGroups, int>
    {
        [MuscleGroups.Abdominals] = 1,
        [MuscleGroups.Obliques] = 1,
        [MuscleGroups.ErectorSpinae] = 1,
        [MuscleGroups.GluteMax] = 0,
        [MuscleGroups.GluteMed | MuscleGroups.GluteMin] = 0,
        [MuscleGroups.GluteMax | MuscleGroups.GluteMed | MuscleGroups.GluteMin] = 1,
        [MuscleGroups.Hamstrings] = 1,
        [MuscleGroups.Quadriceps] = 1,
        [MuscleGroups.FrontDelt] = 0,
        [MuscleGroups.LatDelt] = 0,
        [MuscleGroups.RearDelt] = 0,
        [MuscleGroups.FrontDelt | MuscleGroups.LatDelt | MuscleGroups.RearDelt] = 1,
        [MuscleGroups.Pectorals] = 1,
        [MuscleGroups.Trapezius] = 1,
        [MuscleGroups.LatissimusDorsi] = 1,
        [MuscleGroups.HipFlexors] = 1,
        [MuscleGroups.HipAdductors] = 1,
        [MuscleGroups.Biceps] = 0,
        [MuscleGroups.Triceps] = 0,
        [MuscleGroups.Calves] = 0,
        [MuscleGroups.Forearms] = 0,
        [MuscleGroups.Rhomboids] = 0,
        [MuscleGroups.RotatorCuffs] = 0,
        [MuscleGroups.SerratusAnterior] = 0,
        [MuscleGroups.TibialisAnterior] = 0,
    };
}
