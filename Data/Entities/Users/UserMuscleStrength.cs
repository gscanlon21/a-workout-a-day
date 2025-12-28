using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.Users;

[Table("user_muscle_strength")]
public class UserMuscleStrength
{
    public const int MuscleTargetMin = 0;

    public MusculoskeletalSystem MuscleGroup { get; init; }

    [ForeignKey(nameof(Entities.Users.User.Id))]
    public int UserId { get; init; }

    [JsonIgnore, InverseProperty(nameof(Entities.Users.User.UserMuscleStrengths))]
    public virtual User User { get; private init; } = null!;

    public int Start { get; set; }

    public int End { get; set; }

    [NotMapped]
    public Range Range => new(Start, End);

    public override string ToString()
    {
        return $"{MuscleGroup}: {Range}";
    }

    /// <summary>
    /// The volume each muscle group should be exposed to each week.
    /// 
    /// ~24 per exercise.
    /// 
    /// https://www.bodybuilding.com/content/how-many-exercises-per-muscle-group.html
    /// 50-70 for minor muscle groups.
    /// 90-120 for major muscle groups.
    /// </summary>
    public static readonly IDictionary<MusculoskeletalSystem, Range> MuscleTargets = new Dictionary<MusculoskeletalSystem, Range>
    {
        [MusculoskeletalSystem.Obliques] = 120..160, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MusculoskeletalSystem.Abdominals] = 110..150, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MusculoskeletalSystem.ErectorSpinae] = 100..140, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MusculoskeletalSystem.HipFlexors] = 90..130, // Type 1 (slow-twitch) muscle fibers, for endurance. Keep this even with Glute Max.
        [MusculoskeletalSystem.GluteMax] = 90..130, // Mega muscle. Keep this even with Hip Flexors.
        [MusculoskeletalSystem.Hamstrings] = 80..120, // Major muscle. Keep this even with Quadriceps.
        [MusculoskeletalSystem.Quadriceps] = 80..120, // Major muscle. Keep this even with Hamstrings.
        [MusculoskeletalSystem.Pectorals] = 80..120, // Major muscle. Keep this even with Trapezius.
        [MusculoskeletalSystem.Trapezius] = 80..120, // Major muscle. Keep this even with Pectorals.
        [MusculoskeletalSystem.LatissimusDorsi] = 80..120, // Major muscle.
        [MusculoskeletalSystem.Forearms] = 70..110, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MusculoskeletalSystem.GluteMed | MusculoskeletalSystem.GluteMin] = 60..100, // Major muscle. Keep this even with Hip Adductors.
        [MusculoskeletalSystem.HipAdductors] = 60..100, // Minor muscle. Keep this even with Glute Med/Min.
        [MusculoskeletalSystem.Calves] = 50..90, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MusculoskeletalSystem.Biceps] = 40..80, // Minor muscle. Keep this even with Triceps.
        [MusculoskeletalSystem.Triceps] = 40..80, // Minor muscle. Keep this even with Biceps.
        [MusculoskeletalSystem.RotatorCuffs] = 30..70, // Miniature muscle.
        [MusculoskeletalSystem.SerratusAnterior] = 30..70, // Miniature muscle. Keep this even with Rhomboids.
        [MusculoskeletalSystem.Rhomboids] = 30..70, // Minor muscle. Keep this even with Serratus Anterior.
        [MusculoskeletalSystem.FrontDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MusculoskeletalSystem.LatDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MusculoskeletalSystem.RearDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MusculoskeletalSystem.TibialisAnterior] = 10..50, // Miniature muscle.
    };
}
