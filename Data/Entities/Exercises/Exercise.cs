using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Data.Entities.Users;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Data.Entities.Exercises;

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
    public VocalSkills VocalSkills { get; private set; }

    [Required]
    public VisualSkills VisualSkills { get; private set; }

    [Required]
    public LumbarSkills LumbarSkills { get; private set; }

    [Required]
    public ThoracicSkills ThoracicSkills { get; private set; }

    [Required]
    public CervicalSkills CervicalSkills { get; private set; }

    [Required, DefaultValue(UserConsts.UserProgressionDefault)]
    [Range(UserConsts.UserProgressionMin, UserConsts.UserProgressionMax)]
    public int StartingProgression { get; private init; } = UserConsts.UserProgressionDefault;

    /// <summary>
    /// Notes about the variation (externally shown).
    /// </summary>
    public string? Notes { get; private init; } = null;

    public string? DisabledReason { get; private init; } = null;

    [NotMapped]
    public LumbarSkills[]? LumbarSkillsBinder
    {
        get => Enum.GetValues<LumbarSkills>().Where(e => LumbarSkills.HasFlag(e)).ToArray();
        set => LumbarSkills = value?.Aggregate(LumbarSkills.None, (a, e) => a | e) ?? LumbarSkills.None;
    }

    [NotMapped]
    public CervicalSkills[]? CervicalSkillsBinder
    {
        get => Enum.GetValues<CervicalSkills>().Where(e => CervicalSkills.HasFlag(e)).ToArray();
        set => CervicalSkills = value?.Aggregate(CervicalSkills.None, (a, e) => a | e) ?? CervicalSkills.None;
    }

    [NotMapped]
    public ThoracicSkills[]? ThoracicSkillsBinder
    {
        get => Enum.GetValues<ThoracicSkills>().Where(e => ThoracicSkills.HasFlag(e)).ToArray();
        set => ThoracicSkills = value?.Aggregate(ThoracicSkills.None, (a, e) => a | e) ?? ThoracicSkills.None;
    }

    [NotMapped]
    public VisualSkills[]? VisualSkillsBinder
    {
        get => Enum.GetValues<VisualSkills>().Where(e => VisualSkills.HasFlag(e)).ToArray();
        set => VisualSkills = value?.Aggregate(VisualSkills.None, (a, e) => a | e) ?? VisualSkills.None;
    }

    [NotMapped]
    public VocalSkills[]? VocalSkillsBinder
    {
        get => Enum.GetValues<VocalSkills>().Where(e => VocalSkills.HasFlag(e)).ToArray();
        set => VocalSkills = value?.Aggregate(VocalSkills.None, (a, e) => a | e) ?? VocalSkills.None;
    }


    #region Navigation Properties

    [InverseProperty(nameof(ExercisePrerequisite.Exercise))]
    public virtual ICollection<ExercisePrerequisite> Prerequisites { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(ExercisePrerequisite.PrerequisiteExercise))]
    public virtual ICollection<ExercisePrerequisite> Postrequisites { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(Variation.Exercise))]
    public virtual ICollection<Variation> Variations { get; private init; } = null!;

    [JsonIgnore, InverseProperty(nameof(UserExercise.Exercise))]
    public virtual ICollection<UserExercise> UserExercises { get; private init; } = null!;

    #endregion


    public override int GetHashCode() => HashCode.Combine(Id);
    public override bool Equals(object? obj) => obj is Exercise other
        && other.Id == Id;
}
