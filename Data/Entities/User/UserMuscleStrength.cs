using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;

[Table("user_muscle_strength")]
public class UserMuscleStrength
{
    public const int MuscleTargetMin = 0;

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
    public static readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Abdominals] = 120..240, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 110..220, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 100..200, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.HipFlexors] = 90..130, // Type 1 (slow-twitch) muscle fibers, for endurance. Keep this even with Glute Max.
        [MuscleGroups.GluteMax] = 90..130, // Mega muscle. Keep this even with Hip Flexors.
        [MuscleGroups.Hamstrings] = 80..120, // Major muscle. Keep this even with Quadriceps.
        [MuscleGroups.Quadriceps] = 80..120, // Major muscle. Keep this even with Hamstrings.
        [MuscleGroups.Pectorals] = 80..120, // Major muscle. Keep this even with Trapezius.
        [MuscleGroups.Trapezius] = 80..120, // Major muscle. Keep this even with Pectorals.
        [MuscleGroups.LatissimusDorsi] = 80..120, // Major muscle.
        [MuscleGroups.Forearms] = 70..110, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.GluteMed | MuscleGroups.GluteMin] = 60..100, // Major muscle. Keep this even with Hip Adductors.
        [MuscleGroups.HipAdductors] = 60..100, // Minor muscle. Keep this even with Glute Med/Min.
        [MuscleGroups.Calves] = 50..80, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Biceps] = 40..80, // Minor muscle. Keep this even with Triceps.
        [MuscleGroups.Triceps] = 40..80, // Minor muscle. Keep this even with Biceps.
        [MuscleGroups.RotatorCuffs] = 30..70, // Miniature muscle.
        [MuscleGroups.SerratusAnterior] = 30..70, // Miniature muscle. Keep this even with Rhomboids.
        [MuscleGroups.Rhomboids] = 30..70, // Minor muscle. Keep this even with Serratus Anterior.
        [MuscleGroups.FrontDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.LatDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.RearDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.TibialisAnterior] = 10..50, // Miniature muscle.
    };
}
