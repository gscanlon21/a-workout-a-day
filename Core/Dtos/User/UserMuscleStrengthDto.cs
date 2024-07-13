﻿using Core.Models.Exercise;

namespace Core.Dtos.User;

public class UserMuscleStrengthDto
{
    public MuscleGroups MuscleGroup { get; init; }

    public int Start { get; init; }

    public int End { get; init; }

    public Range Range => new(Start, End);
}
