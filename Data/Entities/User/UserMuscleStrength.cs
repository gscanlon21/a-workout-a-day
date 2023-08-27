﻿using Core.Models.Exercise;
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
        [MuscleGroups.Abdominals] = 80..180, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Obliques] = 80..180, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.ErectorSpinae] = 80..180, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Hamstrings] = 80..120, // Major muscle.
        [MuscleGroups.Quadriceps] = 80..120, // Major muscle.
        [MuscleGroups.Pectorals] = 80..120, // Major muscle.
        [MuscleGroups.Trapezius] = 80..120, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 80..120, // Major muscle.
        [MuscleGroups.GluteMax] = 70..110, // Largest muscle group in the body.
        [MuscleGroups.HipFlexors] = 60..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Calves] = 60..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Forearms] = 60..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.GluteMed | MuscleGroups.GluteMin] = 50..90, // Largest muscle group in the body.
        [MuscleGroups.Rhomboids] = 40..80, // Minor muscle.
        [MuscleGroups.Biceps] = 40..80, // Minor muscle.
        [MuscleGroups.Triceps] = 40..80, // Minor muscle.
        [MuscleGroups.HipAdductors] = 40..80, // Minor muscle.
        [MuscleGroups.FrontDelt] = 30..70, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.LatDelt] = 30..70, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.RearDelt] = 30..70, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.SerratusAnterior] = 20..60, // Miniature muscle.
        [MuscleGroups.RotatorCuffs] = 20..60, // Miniature muscle.
    };
}
