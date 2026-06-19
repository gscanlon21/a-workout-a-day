using Core.Models.Exercise;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Code.Extensions;
using Data.Query.Options;
using Data.Query.Options.Users;
using Microsoft.Extensions.DependencyInjection;
using static Core.Code.Extensions.EnumerableExtensions;
using static Data.Query.Runners.BaseQueryRunner;

namespace Data.Query.Filters;

public class UserQueryFilter : BaseQueryFilter
{
    protected readonly Section section;

    public UserQueryFilter(Section sec)
    {
        section = sec;
    }

    public required UserOptions UserOptions { private get; init; }
    public required ExclusionOptions ExclusionOptions { protected get; init; }
    public required SelectionOptions SelectionOptions { protected get; init; }
    public required MuscleGroupOptions MuscleGroupOptions { protected get; init; }
    public required MovementPatternOptions MovementPatternOptions { protected get; init; }

    public override async Task<List<QueryResults>> Filter(List<InProgressQueryResults> filteredResults, IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
    {
        using var scope = factory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var muscleTarget = MuscleGroupOptions.MuscleTarget.Compile();
        var secondaryMuscleTarget = MuscleGroupOptions.SecondaryMuscleTarget?.Compile();
        var finalResults = new List<QueryResults>();
        do
        {
            foreach (var exercise in filteredResults)
            {
                // Don't choose two exercises that work the same set of muscle groups?
                // Don't choose two exercises that work the same muscle group in isolation?
                var finalResultsExerciseIds = finalResults.Select(fr => fr.Exercise.Id);

                // Don't choose two variations of the same exercise.
                if (finalResultsExerciseIds.Contains(exercise.Exercise.Id))
                {
                    continue;
                }

                // Don't choose if all prerequisites are being worked. 
                if (exercise.Prerequisites.AllIfAny(p => ExclusionOptions.ExerciseIds.Contains(p.Id) || finalResultsExerciseIds.Contains(p.Id)))
                {
                    continue;
                }

                // Don't choose if all postrequisites are being worked.
                if (exercise.Postrequisites.AllIfAny(p => ExclusionOptions.ExerciseIds.Contains(p.Id) || finalResultsExerciseIds.Contains(p.Id)))
                {
                    continue;
                }

                // If the exercise has skills.
                if (exercise.Exercise.VocalSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                    && (finalResults.Aggregate(VocalSkills.None, (c, n) => c | n.Exercise.VocalSkills) & exercise.Exercise.VocalSkills) == exercise.Exercise.VocalSkills)
                {
                    continue;
                }

                // If the exercise has skills.
                if (exercise.Exercise.VisualSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                    && (finalResults.Aggregate(VisualSkills.None, (c, n) => c | n.Exercise.VisualSkills) & exercise.Exercise.VisualSkills) == exercise.Exercise.VisualSkills)
                {
                    continue;
                }

                // If the exercise has skills.
                if (exercise.Exercise.CervicalSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                    && (finalResults.Aggregate(CervicalSkills.None, (c, n) => c | n.Exercise.CervicalSkills) & exercise.Exercise.CervicalSkills) == exercise.Exercise.CervicalSkills)
                {
                    continue;
                }

                // If the exercise has skills.                
                if (exercise.Exercise.ThoracicSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                    && (finalResults.Aggregate(ThoracicSkills.None, (c, n) => c | n.Exercise.ThoracicSkills) & exercise.Exercise.ThoracicSkills) == exercise.Exercise.ThoracicSkills)
                {
                    continue;
                }

                // If the exercise has skills.
                if (exercise.Exercise.LumbarSkills > 0 // Don't choose two variations that work the same skills. Check all flags, not any flag.
                    && (finalResults.Aggregate(LumbarSkills.None, (c, n) => c | n.Exercise.LumbarSkills) & exercise.Exercise.LumbarSkills) == exercise.Exercise.LumbarSkills)
                {
                    continue;
                }

                // Choose exercises that cover a unique movement pattern.
                if (MovementPatternOptions.MovementPatterns.HasValue && MovementPatternOptions.IsUnique)
                {
                    var unworkedMovementPatterns = EnumExtensions.GetValuesExcluding(MovementPattern.None, MovementPattern.All)
                        // The movement pattern has not yet been worked. Checking any flag so we don't double up.
                        .Where(mp => !finalResults.Any(r => mp.HasAnyFlag(r.Variation.MovementPattern)))
                        // The movement pattern is in our list of movement patterns to select from.
                        .Where(v => MovementPatternOptions.MovementPatterns.Value.HasFlag(v));

                    // We've already worked all unique movement patterns.
                    if (!unworkedMovementPatterns.Any())
                    {
                        break;
                    }

                    // If none of the unworked movement patterns match up with the variation's movement patterns.
                    if (!unworkedMovementPatterns.Any(mp => mp.HasAnyFlag(exercise.Variation.MovementPattern)))
                    {
                        continue;
                    }
                }

                // Choose exercises that cover at least X muscles in our targeted muscles set.
                if (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.HasValue)
                {
                    var unworkedMuscleGroups = GetUnworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);

                    // We've already worked all unique muscles.
                    if (unworkedMuscleGroups.Count == 0)
                    {
                        break;
                    }

                    // Find the number of weeks of padding that this variation still has left. If the padded refresh date is earlier than today, then use the number 0.
                    var weeksFromLastSeen = Math.Max(0, (exercise.UserVariation?.LastSeen?.DayNumber ?? DateHelpers.Today.DayNumber) - DateHelpers.Today.DayNumber) / 7;
                    // Allow exercises that have a refresh date since we want to show those continuously until that date.
                    // Allow the first exercise with any muscle group so the user does not get stuck from seeing certain exercises
                    // ... if, for example, a prerequisite only works one muscle group and that muscle group is otherwise worked by compound exercises.
                    var musclesToWork = (exercise.UserVariation?.RefreshAfter != null || !finalResults.Any(e => e.UserVariation?.RefreshAfter == null)) ? 1
                        // Choose two variations with no refresh padding and few muscles worked over a variation with lots of refresh padding and many muscles worked.
                        // Doing weeks out so we still prefer variations with many muscles worked to an extent.
                        : (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.Value + weeksFromLastSeen);

                    // The exercise does not work enough unique muscles that we are trying to target.
                    if (unworkedMuscleGroups.Count(mg => muscleTarget(exercise).HasAnyFlag(mg)) < musclesToWork)
                    {
                        continue;
                    }
                }

                // Don't overwork muscle groups. Run this after MuscleGroups/MovementPatterns so we can break early if there are no muscle groups left to work.
                var overworkedMuscleGroups = GetOverworkedMuscleGroups(finalResults, muscleTarget: muscleTarget, secondaryMuscleTarget: secondaryMuscleTarget);
                if (overworkedMuscleGroups.Any(mg => muscleTarget(exercise).HasAnyFlag(mg)))
                {
                    continue;
                }

                finalResults.Add(new QueryResults(section, exercise.Exercise, exercise.Variation, exercise.UserExercise, exercise.UserVariation, exercise.Prerequisites, exercise.Postrequisites, exercise.EasierVariation, exercise.HarderVariation, UserOptions.Intensity));
                if (finalResults.Count >= take)
                {
                    break;
                }
            }
        }
        // If AtLeastXUniqueMusclesPerExercise is say 4 and there are 7 muscle groups, we don't want 3 isolation exercises at the end if there are no 3-muscle group compound exercises to find.
        // Choose a 3-muscle group compound exercise and then choose a 2-muscle group compound exercise and then choose an isolation exercise.
        while (MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise.HasValue && --MuscleGroupOptions.AtLeastXUniqueMusclesPerExercise >= 1);

        return orderBy switch
        {
            OrderBy.ProgressionLevels => [
                // Not in a workout context, order by progression levels.
                .. finalResults.OrderBy(vm => vm.Variation.Progression.Min, NullOrder.NullsFirst)
                    .ThenBy(vm => vm.Variation.Progression.Max, NullOrder.NullsLast)
                    .ThenBy(vm => vm.Variation.Name)
            ],
            OrderBy.LeastDifficultFirst => [
                // Order by least expected difficulty first.
                .. finalResults.OrderBy(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.MusclesTargeted => [
                // Show exercises that work a muscle target we want more of first.
                .. finalResults.OrderByDescending(vm => (muscleTarget(vm) & MuscleGroupOptions.AllMuscleGroups).PopCount())
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.CoreLast => [
                // Core exercises last.
                .. finalResults.OrderBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then plyometrics. They're best done early in the workout when the user isn't fatigued.
                    .ThenByDescending(vm => vm.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Speed))
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            OrderBy.PlyometricsFirst => [
                // Order plyometrics first. They're best done early in the workout when the user isn't fatigued.
                .. finalResults.OrderByDescending(vm => vm.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Speed))
                    // Core exercises last. Ordering exercises that don't work core muscles first.
                    .ThenBy(vm => (muscleTarget(vm) & MusculoskeletalSystem.Core).PopCount() >= 2)
                    // Then by hardest expected difficulty to easiest expected difficulty.
                    .ThenByDescending(vm => muscleTarget(vm).PopCount())
            ],
            _ => finalResults // We are in a workout context, keep the result order.
        };
    }

    /// <summary>
    /// Calculates what muscle groups haven't yet been worked by the <paramref name="finalResults"/>.
    /// </summary>
    private List<MusculoskeletalSystem> GetUnworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        return MuscleGroupOptions.MuscleTargetsRDA.Where(kv =>
        {
            // We are targeting this muscle group.
            var workedCount = finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have not overworked this muscle group and this muscle group is a part of our worked set.
            return workedCount < kv.Value && MuscleGroupOptions.AllMuscleGroups.HasFlag(kv.Key);
        }).Select(kv => kv.Key).ToList();
    }

    /// <summary>
    /// Calculates what muscle groups have been overworked by the <paramref name="finalResults"/>.
    /// </summary>
    private List<MusculoskeletalSystem> GetOverworkedMuscleGroups(IList<QueryResults> finalResults, Func<IExerciseVariationCombo, MusculoskeletalSystem> muscleTarget, Func<IExerciseVariationCombo, MusculoskeletalSystem>? secondaryMuscleTarget = null)
    {
        // Not checking if this muscle group is a part of our worked set.
        // We don't want to overwork any muscle regardless if we are targeting it.
        return MuscleGroupOptions.MuscleTargetsTUL.Where(kv =>
        {
            var workedCount = finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: muscleTarget);
            if (secondaryMuscleTarget != null)
            {
                // Weight secondary muscles as half.
                workedCount += finalResults.WorkedAnyMuscleVolume(kv.Key, muscleTarget: secondaryMuscleTarget, weightDivisor: 2);
            }

            // We have overworked this muscle group.
            return workedCount > kv.Value;
        }).Select(kv => kv.Key).ToList();
    }
}
