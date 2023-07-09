using Core.Consts;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle_mobility")]
public class UserMuscleMobility
{
    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserMuscleMobilities))]
    public virtual User User { get; private init; } = null!;

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
        [MuscleGroups.Glutes] = 1,
        [MuscleGroups.Hamstrings] = 1,
        [MuscleGroups.Quadriceps] = 1,
        [MuscleGroups.Deltoids] = 1,
        [MuscleGroups.Pectorals] = 1,
        [MuscleGroups.Trapezius] = 1,
        [MuscleGroups.LatissimusDorsi] = 1,
        [MuscleGroups.HipFlexors] = 1,
        [MuscleGroups.Calves] = 1,
        [MuscleGroups.Forearms] = 0,
        [MuscleGroups.Rhomboids] = 0,
        [MuscleGroups.Biceps] = 1,
        [MuscleGroups.Triceps] = 1,
        [MuscleGroups.SerratusAnterior] = 0,
        [MuscleGroups.RotatorCuffs] = 0,
        [MuscleGroups.HipAdductors] = 1,
        [MuscleGroups.TibialisAnterior] = 0,
    };
}
