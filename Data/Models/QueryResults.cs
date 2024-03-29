﻿using Core.Models.Newsletter;
using Data.Dtos.Newsletter;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Query;
using System.Diagnostics;

namespace Data.Models;

[DebuggerDisplay("{Exercise}: {Variation}")]
public record QueryResults(
    Section Section,
    Exercise Exercise,
    Variation Variation,
    UserExercise? UserExercise,
    UserVariation? UserVariation,
    IList<ExercisePrerequisiteDto> ExercisePrerequisites,
    (string? name, string? reason) EasierVariation,
    (string? name, string? reason) HarderVariation
) : IExerciseVariationCombo;