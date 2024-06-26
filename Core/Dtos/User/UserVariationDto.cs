﻿using Core.Consts;
using Core.Models.Newsletter;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Core.Dtos.User;

/// <summary>
/// User's intensity stats.
/// </summary>
[Table("user_variation")]
[DebuggerDisplay("User: {UserId}, Variation: {VariationId}")]
public class UserVariationDto
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    [Required]
    public int UserId { get; init; }

    [Required]
    public int VariationId { get; init; }

    public string? Notes { get; set; }

    [Required]
    public Section Section { get; set; }

    /// <summary>
    /// Don't show this variation to the user.
    /// </summary>
    [Required]
    public bool Ignore { get; set; }

    /// <summary>
    /// When was this exercise last seen in the user's newsletter.
    /// </summary>
    [Required]
    public DateOnly LastSeen { get; set; }

    /// <summary>
    /// If this is set, will not update the LastSeen date until this date is reached.
    /// This is so we can reduce the variation of workouts and show the same groups of exercises for a month+ straight.
    /// </summary>
    public DateOnly? RefreshAfter { get; set; }

    /// <summary>
    /// How often to refresh exercises.
    /// </summary>
    [Required, Range(UserConsts.LagRefreshXWeeksMin, UserConsts.LagRefreshXWeeksMax)]
    public int LagRefreshXWeeks { get; set; } = UserConsts.LagRefreshXWeeksDefault;

    /// <summary>
    /// How often to refresh exercises.
    /// </summary>
    [Required, Range(UserConsts.PadRefreshXWeeksMin, UserConsts.PadRefreshXWeeksMax)]
    public int PadRefreshXWeeks { get; set; } = UserConsts.PadRefreshXWeeksDefault;

    /// <summary>
    /// How much weight the user is able to lift.
    /// </summary>
    [Range(UserConsts.UserWeightMin, UserConsts.UserWeightMax)]
    public int Weight { get; set; } = UserConsts.UserWeightDefault;

    [Range(UserConsts.UserSetsMin, UserConsts.UserSetsMax)]
    public int Sets { get; set; } = UserConsts.UserSetsDefault;

    [Range(UserConsts.UserRepsMin, UserConsts.UserRepsMax)]
    public int Reps { get; set; } = UserConsts.UserRepsDefault;

    [JsonIgnore]
    public virtual UserDto User { get; init; } = null!;

    [JsonIgnore]
    public virtual Exercise.VariationDto Variation { get; init; } = null!;

    [JsonIgnore]
    public virtual ICollection<UserVariationWeightDto> UserVariationWeights { get; init; } = [];

    public override int GetHashCode() => HashCode.Combine(UserId, VariationId, Section);

    public override bool Equals(object? obj) => obj is UserVariationDto other
        && other.VariationId == VariationId
        && other.UserId == UserId
        && other.Section == Section;
}
