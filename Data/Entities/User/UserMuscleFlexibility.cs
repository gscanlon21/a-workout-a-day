using Core.Consts;
using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle_flexibility")]
public class UserMuscleFlexibility
{
    public MusculoskeletalSystem MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserMuscleFlexibilities))]
    public virtual User User { get; private init; } = null!;

    [Range(UserConsts.UserMuscleFlexibilityMin, UserConsts.UserMuscleFlexibilityMax)]
    public int Count { get; set; }

    public override string ToString()
    {
        return $"{MuscleGroup}: {Count}";
    }

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// </summary>
    public static readonly IDictionary<MusculoskeletalSystem, int> MuscleTargets = new Dictionary<MusculoskeletalSystem, int>
    {
        [MusculoskeletalSystem.Abdominals] = 1,
        [MusculoskeletalSystem.Obliques] = 1,
        [MusculoskeletalSystem.ErectorSpinae] = 1,
        [MusculoskeletalSystem.GluteMax] = 0,
        [MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin] = 0,
        [MusculoskeletalSystem.GluteMax | MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin] = 1,
        [MusculoskeletalSystem.Hamstrings] = 1,
        [MusculoskeletalSystem.Quadriceps] = 1,
        [MusculoskeletalSystem.FrontDelt] = 0,
        [MusculoskeletalSystem.LatDelt] = 0,
        [MusculoskeletalSystem.RearDelt] = 0,
        [MusculoskeletalSystem.FrontDelt | MusculoskeletalSystem.LatDelt | MusculoskeletalSystem.RearDelt] = 1,
        [MusculoskeletalSystem.Pectorals] = 1,
        [MusculoskeletalSystem.Trapezius] = 1,
        [MusculoskeletalSystem.LatissimusDorsi] = 1,
        [MusculoskeletalSystem.HipFlexors] = 1,
        [MusculoskeletalSystem.HipAdductors] = 1,
        [MusculoskeletalSystem.Biceps] = 0,
        [MusculoskeletalSystem.Triceps] = 0,
        [MusculoskeletalSystem.Calves] = 0,
        [MusculoskeletalSystem.Forearms] = 0,
        [MusculoskeletalSystem.Rhomboids] = 0,
        [MusculoskeletalSystem.RotatorCuffs] = 0,
        [MusculoskeletalSystem.SerratusAnterior] = 0,
        [MusculoskeletalSystem.TibialisAnterior] = 0,
    };
}
