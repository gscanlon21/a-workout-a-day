﻿using Core.Models.Exercise;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

[Table("user_muscle_strength")]
public class UserMuscleStrengthDto
{
    public const int MuscleTargetMin = 0;

    public MuscleGroups MuscleGroup { get; init; }

    public int UserId { get; init; }

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;

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
        [MuscleGroups.GluteMax] = 90..130, // Mega muscle.
        [MuscleGroups.HipFlexors] = 80..120, // Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Hamstrings] = 80..120, // Major muscle.
        [MuscleGroups.Quadriceps] = 80..120, // Major muscle.
        [MuscleGroups.Pectorals] = 80..120, // Major muscle.
        [MuscleGroups.Trapezius] = 80..120, // Major muscle.
        [MuscleGroups.LatissimusDorsi] = 80..120, // Major muscle.
        [MuscleGroups.GluteMed | MuscleGroups.GluteMin] = 70..110, // Major muscle.
        [MuscleGroups.Calves] = 60..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.Forearms] = 60..100, // Minor muscle. Type 1 (slow-twitch) muscle fibers, for endurance.
        [MuscleGroups.HipAdductors] = 50..90, // Minor muscle.
        [MuscleGroups.Biceps] = 40..80, // Minor muscle.
        [MuscleGroups.Triceps] = 40..80, // Minor muscle.
        [MuscleGroups.Rhomboids] = 30..70, // Minor muscle.
        [MuscleGroups.RotatorCuffs] = 30..70, // Miniature muscle.
        [MuscleGroups.FrontDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.LatDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.RearDelt] = 20..60, // Major muscle. The deltoids are used in almost every arm movement humans can complete.
        [MuscleGroups.SerratusAnterior] = 10..50, // Miniature muscle.
        [MuscleGroups.TibialisAnterior] = 0..40, // Miniature muscle.
    };
}
