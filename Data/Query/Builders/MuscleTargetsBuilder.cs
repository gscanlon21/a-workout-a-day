using Core.Code.Extensions;
using Core.Consts;
using Core.Models.Exercise;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query.Options;

namespace Data.Query.Builders;

public interface IMuscleGroupBuilderTargets
{
    IMuscleGroupBuilderFinal WithoutMuscleTargets();
    IMuscleGroupBuilderFinal WithMuscleTargets(IDictionary<MuscleGroups, int> muscleTargets);
    IMuscleGroupBuilderFinal WithMuscleTargetsFromMuscleGroups(IDictionary<MuscleGroups, int>? workedMusclesDict = null);
}

public interface IMuscleGroupBuilderFinal
{
    MuscleGroupOptions Build();

    IMuscleGroupBuilderFinal AdjustMuscleTargets(WorkoutContext context, bool adjustUp = true, bool adjustDown = true);
}

/// <summary>
/// Step-builder pattern for muscle target options.
/// </summary>
public class MuscleTargetsBuilder : IOptions, IMuscleGroupBuilderTargets, IMuscleGroupBuilderFinal
{
    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public MuscleGroups MuscleGroups = MuscleGroups.None;

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public IDictionary<MuscleGroups, int> MuscleTargets = new Dictionary<MuscleGroups, int>();

    private MuscleTargetsBuilder(MuscleGroups muscleGroups)
    {
        MuscleGroups = muscleGroups;
    }

    public static IMuscleGroupBuilderTargets WithMuscleGroups(MuscleGroups muscleGroups)
    {
        return new MuscleTargetsBuilder(muscleGroups);
    }

    public IMuscleGroupBuilderFinal WithoutMuscleTargets()
    {
        return this;
    }

    public IMuscleGroupBuilderFinal WithMuscleTargets(IDictionary<MuscleGroups, int> muscleTargets)
    {
        MuscleTargets = muscleTargets;
        return this;
    }

    public IMuscleGroupBuilderFinal WithMuscleTargetsFromMuscleGroups(IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        MuscleTargets = EnumExtensions.GetSingleValues32<MuscleGroups>()
            // Base 1 target for each targeted muscle group. If we've already worked this muscle, reduce the muscle target volume.
            // Keep all muscle groups in our target dict so we exclude overworked muscles.
            .ToDictionary(mg => mg, mg => (MuscleGroups.HasFlag(mg) ? 1 : 0) - (workedMusclesDict?.TryGetValue(mg, out int workedAmt) ?? false ? workedAmt : 0));

        return this;
    }

    /// <summary>
    /// Adjustments to the muscle groups to reduce muscle imbalances.
    /// </summary>
    public IMuscleGroupBuilderFinal AdjustMuscleTargets(WorkoutContext context, bool adjustUp = true, bool adjustDown = true)
    {
        IDictionary<MuscleGroups, Range> userMuscleTargetDefaults = UserMuscleStrength.MuscleTargets(context.User);
        if (context.WeeklyMuscles != null)
        {
            foreach (var key in MuscleTargets.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (context.WeeklyMuscles[key].HasValue && userMuscleTargetDefaults.ContainsKey(key))
                {
                    // Use the default muscle target when the user's workout split never targets this muscle group--because they can't adjust this muscle group's muscle target.
                    var targetRange = (context.UserAllWorkedMuscles.HasFlag(key)
                        ? context.User.UserMuscleStrengths.FirstOrDefault(um => um.MuscleGroup == key)?.Range
                        : null) ?? userMuscleTargetDefaults[key];

                    // Don't be so harsh about what constitutes an out-of-range value when there is not a lot of weekly data to work with.
                    var middle = (targetRange.Start.Value + targetRange.End.Value) / 2;
                    var adjustBy = Math.Max(1, ExerciseConsts.TargetVolumePerExercise - Convert.ToInt32(context.WeeklyMusclesWeeks));
                    var adjustmentRange = new Range(targetRange.Start.Value, Math.Max(middle, targetRange.End.Value - adjustBy));
                    var outOfRangeIncrement = Convert.ToInt32(ExerciseConsts.TargetVolumePerExercise / Math.Max(1, context.WeeklyMusclesWeeks));

                    // We don't work this muscle group often enough
                    if (adjustUp && context.WeeklyMuscles[key] < adjustmentRange.Start.Value)
                    {
                        // Cap the muscle targets so we never get more than 2 accessory exercises a day for a specific muscle group.
                        MuscleTargets[key] = Math.Min(
                            // If we've already worked this muscle, lessen the volume we cap at.
                            Math.Min(2, 2 + MuscleTargets[key]), 
                            MuscleTargets[key] + (adjustmentRange.Start.Value - context.WeeklyMuscles[key].GetValueOrDefault()) / outOfRangeIncrement + 1
                        );
                    }
                    // We work this muscle group too often
                    else if (adjustDown && context.WeeklyMuscles[key] > adjustmentRange.End.Value)
                    {
                        // -1 means we don't choose any exercises that work this muscle. 0 means we don't specifically target this muscle, but exercises working other muscles may still be picked.
                        MuscleTargets[key] = Math.Max(-1, MuscleTargets[key] - (context.WeeklyMuscles[key].GetValueOrDefault() - adjustmentRange.End.Value) / outOfRangeIncrement - 1);
                    }
                }
            }
        }

        return this;
    }

    public MuscleGroupOptions Build()
    {
        return new MuscleGroupOptions(MuscleGroups, MuscleTargets);
    }
}