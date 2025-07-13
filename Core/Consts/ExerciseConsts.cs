
namespace Core.Consts;

public static class ExerciseConsts
{
    /// <summary>
    /// ~24 per exercise: 6reps * 4sets; 8reps * 3sets; 12reps * 2sets; 60s total TUT / 2.5.
    /// </summary>
    public const double TargetVolumePerExercise = 24;

    /// <summary>
    /// How many days until exercises become stale if they haven't been able to be seen.
    /// </summary>
    public const int StaleAfterDays = 7;

    public const int AtLeastXUniqueMusclesPerExerciseMin = 1;
    public const int AtLeastXUniqueMusclesPerExerciseMax = 9;

    /// <summary>
    /// The maximum number of prehab exercises to show in a workout.
    /// </summary>
    public const int MaxPrehabExercisesPerWorkout = 6;
}
