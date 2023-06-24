using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle")]
public class UserMuscle
{
    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserMuscles))]
    public virtual User User { get; private init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    [NotMapped]
    public Range Range => new(Start, End);

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// 
    /// ~24 per exercise.
    /// 
    /// https://www.bodybuilding.com/content/how-many-exercises-per-muscle-group.html
    /// 50-70 for minor muscle groups.
    /// 90-120 for major muscle groups.
    /// </summary>
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 100..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Glutes] = 90..170, // Largest muscle group in the body.
        [MuscleGroups.Hamstrings] = 90..150, // Major muscle.
        [MuscleGroups.Quadriceps] = 90..150, // Major muscle.
        [MuscleGroups.Deltoids] = 90..150, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.Pectorals] = 90..150, // Major muscle.
        [MuscleGroups.Trapezius] = 90..150, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 90..150, // Major muscle.
        [MuscleGroups.HipFlexors] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Calves] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Forearms] = 50..120, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Rhomboids] = 50..90, // Minor muscle.
        [MuscleGroups.Biceps] = 50..90, // Minor muscle.
        [MuscleGroups.Triceps] = 50..90, // Minor muscle.
        [MuscleGroups.SerratusAnterior] = 30..70, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 30..70, // Miniature muscle.
        [MuscleGroups.HipAdductors] = 30..70, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..50, // Generally doesn't require strengthening. 
    };
}
