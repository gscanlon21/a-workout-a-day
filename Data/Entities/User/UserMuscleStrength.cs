using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle_strength")]
public class UserMuscleStrength
{
    public const int MuscleTargetMin = 0;
    public static int MuscleTargetMax(User user) => MuscleTargets(user).Values.MaxBy(v => v.End.Value).End.Value;

    public MuscleGroups MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.User.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserMuscleStrengths))]
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
    public static IDictionary<MuscleGroups, Range> MuscleTargets(User user)
    {
        if (user.IsNewToFitness)
        {
            return new Dictionary<MuscleGroups, Range>
            {
                [MuscleGroups.Abdominals] = 40..80, // Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.Obliques] = 40..80, // Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.ErectorSpinae] = 40..80, // Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.Glutes] = 40..70, // Largest muscle group in the body.
                [MuscleGroups.Hamstrings] = 40..60, // Major muscle.
                [MuscleGroups.Quadriceps] = 40..60, // Major muscle.
                [MuscleGroups.Deltoids] = 40..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
                [MuscleGroups.Pectorals] = 40..60, // Major muscle.
                [MuscleGroups.Trapezius] = 40..60, // Major muscle.
                [MuscleGroups.LatissimusDorsi] = 40..60, // Major muscle.
                [MuscleGroups.HipFlexors] = 20..50, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.Calves] = 20..50, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.Forearms] = 20..50, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
                [MuscleGroups.Rhomboids] = 20..40, // Minor muscle.
                [MuscleGroups.Biceps] = 20..40, // Minor muscle.
                [MuscleGroups.Triceps] = 20..40, // Minor muscle.
                [MuscleGroups.HipAdductors] = 20..40, // Minor muscle.
                [MuscleGroups.SerratusAnterior] = 10..30, // Miniature muscle.
                [MuscleGroups.RotatorCuffs] = 10..30, // Miniature muscle.
            };
        }

        return new Dictionary<MuscleGroups, Range>
        {
            [MuscleGroups.Abdominals] = 80..160, // Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.Obliques] = 80..160, // Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.ErectorSpinae] = 80..160, // Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.Glutes] = 80..140, // Largest muscle group in the body.
            [MuscleGroups.Hamstrings] = 80..120, // Major muscle.
            [MuscleGroups.Quadriceps] = 80..120, // Major muscle.
            [MuscleGroups.Deltoids] = 80..120, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
            [MuscleGroups.Pectorals] = 80..120, // Major muscle.
            [MuscleGroups.Trapezius] = 80..120, // Major muscle.
            [MuscleGroups.LatissimusDorsi] = 80..120, // Major muscle.
            [MuscleGroups.HipFlexors] = 40..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.Calves] = 40..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.Forearms] = 40..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
            [MuscleGroups.Rhomboids] = 40..80, // Minor muscle.
            [MuscleGroups.Biceps] = 40..80, // Minor muscle.
            [MuscleGroups.Triceps] = 40..80, // Minor muscle.
            [MuscleGroups.HipAdductors] = 40..80, // Minor muscle.
            [MuscleGroups.SerratusAnterior] = 20..60, // Miniature muscle.
            [MuscleGroups.RotatorCuffs] = 20..60, // Miniature muscle.
        };
    }
}
