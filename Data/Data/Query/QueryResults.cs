using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using System.Diagnostics;

namespace Data.Data.Query;

[DebuggerDisplay("{Exercise}: {Variation}")]
public record QueryResults(
    Section Section,
    Exercise Exercise,
    Variation Variation,
    ExerciseVariation ExerciseVariation,
    UserExercise? UserExercise,
    UserExerciseVariation? UserExerciseVariation,
    UserVariation? UserVariation,
    (string? name, string? reason) EasierVariation,
    (string? name, string? reason) HarderVariation
) : IExerciseVariationCombo;