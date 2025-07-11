﻿using Core.Models.Exercise;
using Core.Models.User;

namespace Core.Dtos.Newsletter;

/// <summary>
/// DTO for UserWorkout.cs
/// </summary>
public class UserWorkoutDto
{
    /// <summary>
    /// The date the workout is for, using the user's UTC offset date.
    /// </summary>
    public DateOnly Date { get; init; }

    /// <summary>
    /// What day of the workout split was used?
    /// </summary>
    public WorkoutRotationDto Rotation { get; set; } = null!;

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Frequency Frequency { get; init; }

    /// <summary>
    /// What was the workout split used when this newsletter was sent?
    /// </summary>
    public Intensity Intensity { get; init; }

    /// <summary>
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate
    /// </summary>
    public bool IsDeloadWeek { get; init; }

    public string? Logs { get; init; }
}
