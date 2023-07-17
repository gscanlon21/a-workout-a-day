﻿using Data.Entities.Exercise;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Data.Entities.User;


/// <summary>
/// User's progression level of an exercise.
/// 
/// TODO Scopes.
/// TODO Single-use tokens.
/// </summary>
[Table("user_variation_weight"), Comment("User variation weight log")]
public class UserVariationWeight
{
    public UserVariationWeight() { }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    [Required]
    public int Weight { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    public int VariationId { get; set; }

    /// <summary>
    /// The token should stop working after this date.
    /// </summary>
    [Required]
    public DateOnly Date { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);

    [JsonIgnore, InverseProperty(nameof(Entities.User.User.UserVariationWeights))]
    public virtual User User { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Exercise.Variation.UserVariationWeights))]
    public virtual Variation Variation { get; private init; } = null!;
}
