using Web.Models.Exercise;

namespace Web.ViewModels.User;

/// <summary>
/// Viewmodel for MonthlyMuscles.cshtml
/// </summary>
public class MonthlyMusclesViewModel
{
    public required Entities.User.User User { get; set; }

    public required IDictionary<MuscleGroups, int> WeeklyTimeUnderTension { get; set; }

    public required double WeeklyTimeUnderTensionAvg { get; set; }

    /// <summary>
    /// The avg minimum number of seconds per week each muscle group should be under tension.
    /// </summary>
    public const double MinSecsPerWeek = 200d;

    /// <summary>
    /// The avg maximum number of seconds per week each muscle group should be under tension.
    /// </summary>
    public const double MaxSecsPerWeek = 300d;

    /// <summary>
    /// The range of time under tension each musclegroup should be exposed to each week.
    /// </summary>
    public readonly IDictionary<MuscleGroups, Range> MuscleTargets = new Dictionary<MuscleGroups, Range>
    {
        [MuscleGroups.Glutes] = 250..500,
        [MuscleGroups.Abdominals] = 250..500,
        [MuscleGroups.Obliques] = 200..400,
        [MuscleGroups.Pectorals] = 200..400,
        [MuscleGroups.Deltoids] = 200..400,
        [MuscleGroups.Hamstrings] = 200..400,
        [MuscleGroups.Quadriceps] = 200..400,
        [MuscleGroups.LatissimusDorsi] = 200..400,
        [MuscleGroups.Trapezius] = 200..400,
        [MuscleGroups.Rhomboids] = 175..350,
        [MuscleGroups.ErectorSpinae] = 175..350,
        [MuscleGroups.Forearms] = 150..300,
        [MuscleGroups.Calves] = 150..300,
        [MuscleGroups.HipFlexors] = 150..300,
        [MuscleGroups.HipAdductors] = 150..300,
        [MuscleGroups.Biceps] = 125..250,
        [MuscleGroups.Triceps] = 125..250,
        [MuscleGroups.SerratusAnterior] = 100..200,
        [MuscleGroups.RotatorCuffs] = 100..200
    };
}
