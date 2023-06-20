using Data.Entities.Exercise;
using Data.Entities.User;
using System.Diagnostics;

namespace Data.Data.Query;

[DebuggerDisplay("{Exercise}: {Variation}")]
public record QueryResults(
    User? User,
    Exercise Exercise,
    Variation Variation,
    ExerciseVariation ExerciseVariation,
    UserExercise? UserExercise,
    UserExerciseVariation? UserExerciseVariation,
    UserVariation? UserVariation,
    Tuple<string?, string?>? EasierVariation,
    Tuple<string?, string?>? HarderVariation
) : IExerciseVariationCombo;