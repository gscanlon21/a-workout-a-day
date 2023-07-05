﻿using Core.Models.Exercise;

namespace Data.Data.Query.Options;

public class ExerciseFocusOptions : IOptions
{
    public ExerciseFocusOptions() { }

    public ExerciseFocusOptions(ExerciseFocus? exerciseFocus)
    {
        ExerciseFocus = exerciseFocus;
    }

    public ExerciseFocus? ExerciseFocus { get; set; }
}