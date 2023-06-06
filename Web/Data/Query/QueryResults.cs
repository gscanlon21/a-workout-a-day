using System.Diagnostics;
using Web.Entities.Exercise;
using Web.Entities.User;

namespace Web.Data.Query;

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