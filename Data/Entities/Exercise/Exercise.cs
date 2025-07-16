﻿using Core.Models.Exercise.Skills;
using Data.Entities.User;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercise;

/// <summary>
/// Exercises listed on the website
/// </summary>
[Table("exercise")]
[DebuggerDisplay("{Name,nq}")]
public class Exercise
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; private init; }

    /// <summary>
    /// Friendly name.
    /// </summary>
    [Required]
    public string Name { get; private init; } = null!;

    [Required]
    public VisualSkills VisualSkills { get; private init; }

    [Required]
    public CervicalSkills CervicalSkills { get; private init; }

    [Required]
    public ThoracicSkills ThoracicSkills { get; private init; }

    [Required]
    public LumbarSkills LumbarSkills { get; private init; }

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; private init; } = null;

    public string? DisabledReason { get; private init; } = null;

    [InverseProperty(nameof(ExercisePrerequisite.Exercise))]
    public virtual ICollection<ExercisePrerequisite> Prerequisites { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
    public virtual ICollection<ExercisePrerequisite> Postrequisites { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Variation.Exercise))]
    public virtual ICollection<Variation> Variations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserExercise.Exercise))]
    public virtual ICollection<UserExercise> UserExercises { get; private init; } = null!;

    public override int GetHashCode() => HashCode.Combine(Id);

    public override bool Equals(object? obj) => obj is Exercise other
        && other.Id == Id;
}
