using Core.Consts;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle_flexibility")]
public class UserMuscleFlexibility
{
    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserMuscleFlexibilities))]
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
        [MuscleGroups.HipAdductors] = 1,
        [MuscleGroups.Biceps] = 0,
        [MuscleGroups.Triceps] = 0,
        [MuscleGroups.Calves] = 0,
        [MuscleGroups.Forearms] = 0,
        [MuscleGroups.Rhomboids] = 0,
        [MuscleGroups.RotatorCuffs] = 0,
        [MuscleGroups.SerratusAnterior] = 0,

        // Not working these: (They are too small and limited in what stretches them, so they are better targeted by the prehab section).
        //[MuscleGroups.TibialisAnterior] = 0,
    };
}
