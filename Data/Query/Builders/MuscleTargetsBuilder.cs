using Core.Code;
using Core.Consts;
using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.User;
using Data.Query.Options;

namespace Data.Query.Builders;

public interface IMuscleGroupBuilderNoContext
{
    IMuscleGroupBuilderFinalNoContext WithoutMuscleTargets();
}

public interface IMuscleGroupBuilderTargets : IMuscleGroupBuilderNoContext
{
    IMuscleGroupBuilderFinal WithMuscleTargets(IDictionary<MuscleGroups, int> muscleTargets);
    IMuscleGroupBuilderFinal WithMuscleTargetsFromMuscleGroups(IDictionary<MuscleGroups, int>? workedMusclesDict = null);
}

public interface IMuscleGroupBuilderFinalNoContext
{
    MuscleGroupOptions Build(Section section);
}

public interface IMuscleGroupBuilderFinal : IMuscleGroupBuilderFinalNoContext
{
    IMuscleGroupBuilderFinal AdjustMuscleTargets(bool adjustUp = true, bool adjustDown = true, bool adjustDownBuffer = true, IList<WorkoutRotationDto>? rotations = null);
}

/// <summary>
/// Step-builder pattern for muscle target options.
/// </summary>
public class MuscleTargetsBuilder : IOptions, IMuscleGroupBuilderNoContext, IMuscleGroupBuilderFinalNoContext, IMuscleGroupBuilderTargets, IMuscleGroupBuilderFinal
{
    private readonly WorkoutContext? Context;

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public IList<MuscleGroups> MuscleGroups = [];

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public IDictionary<MuscleGroups, int> MuscleTargetsRDA = new Dictionary<MuscleGroups, int>();

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public IDictionary<MuscleGroups, int> MuscleTargetsTUL = new Dictionary<MuscleGroups, int>();

    private MuscleTargetsBuilder(IList<MuscleGroups> muscleGroups, WorkoutContext? context)
    {
        MuscleGroups = muscleGroups;
        Context = context;
    }

    public static IMuscleGroupBuilderNoContext WithMuscleGroups(IList<MuscleGroups> muscleGroups)
    {
        return new MuscleTargetsBuilder(muscleGroups, null);
    }

    public static IMuscleGroupBuilderTargets WithMuscleGroups(WorkoutContext context, IList<MuscleGroups> muscleGroups)
    {
        return new MuscleTargetsBuilder(muscleGroups, context);
    }

    public IMuscleGroupBuilderFinalNoContext WithoutMuscleTargets()
    {
        return this;
    }

    public IMuscleGroupBuilderFinal WithMuscleTargets(IDictionary<MuscleGroups, int> muscleTargets)
    {
        MuscleTargetsRDA = muscleTargets;
        MuscleTargetsTUL = muscleTargets;

        return this;
    }

    public IMuscleGroupBuilderFinal WithMuscleTargetsFromMuscleGroups(IDictionary<MuscleGroups, int>? workedMusclesDict = null)
    {
        // Base 1 target for each targeted muscle group. If we've already worked this muscle, reduce the muscle target volume.
        // Keep all muscle groups in our target dict so we exclude overworked muscles.
        MuscleTargetsRDA = UserMuscleStrength.MuscleTargets.Keys.ToDictionary(mt => mt, mt => (MuscleGroups.Any(mg => mt.HasFlag(mg)) ? 1 : 0) - (workedMusclesDict?.TryGetValue(mt, out int workedAmt) ?? false ? workedAmt : 0));

        // Base 1 target for each targeted muscle group. If we've already worked this muscle, reduce the muscle target volume.
        // Keep all muscle groups in our target dict so we exclude overworked muscles.
        MuscleTargetsTUL = UserMuscleStrength.MuscleTargets.Keys.ToDictionary(mt => mt, mt => (MuscleGroups.Any(mg => mt.HasFlag(mg)) ? 1 : 0) - (workedMusclesDict?.TryGetValue(mt, out int workedAmt) ?? false ? workedAmt : 0));

        return this;
    }

    /// <summary>
    /// Adjustments to the muscle groups to reduce muscle imbalances.
    /// Note: Don't change too much during deload weeks or they don't throw off the weekly muscle target tracking.
    /// </summary>
    public IMuscleGroupBuilderFinal AdjustMuscleTargets(bool adjustUp = true, bool adjustDown = true, bool adjustDownBuffer = true, IList<WorkoutRotationDto>? rotations = null)
    {
        if (Context?.WeeklyMusclesRDA != null)
        {
            foreach (var key in MuscleTargetsRDA.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (adjustUp && Context.WeeklyMusclesRDA[key].HasValue)
                {
                    // We want a buffer before excluding muscle groups to where we don't target the muscle group, but still allow exercises that target the muscle to be chosen.
                    // Forearms, for example, are rarely something we want to target directly, since they are worked in many functional movements.
                    if (adjustDownBuffer && int.IsNegative(Context.WeeklyMusclesRDA[key]!.Value))
                    {
                        MuscleGroups.Remove(key);
                    }

                    // Reduce the scale when the user has less workouts in a week.
                    var workoutsTargetingMuscleGroupPerWeek = rotations?.Count(r => r.MuscleGroupsWithCore.Contains(key)) ?? Context.User.SendDays.PopCount();
                    var target = (int)Math.Ceiling(Context.WeeklyMusclesRDA[key]!.Value / (double)workoutsTargetingMuscleGroupPerWeek / ExerciseConsts.TargetVolumePerExercise);
                    // Cap the muscle targets so we never get more than 3 accessory exercises a day for a specific muscle group.
                    // If we've already worked this muscle, lessen the volume we cap at.
                    MuscleTargetsRDA[key] = Math.Min(2 + MuscleTargetsRDA[key], MuscleTargetsRDA[key] + target);
                }
            }
        }

        if (Context?.WeeklyMusclesTUL != null)
        {
            foreach (var key in MuscleTargetsTUL.Keys)
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks.
                if (adjustDown && Context.WeeklyMusclesTUL[key].HasValue)
                {
                    // -1 means we don't choose any exercises that work this muscle. 0 means we don't specifically target this muscle, but exercises working other muscles may still be picked.
                    MuscleTargetsTUL[key] += Context.WeeklyMusclesTUL[key]!.Value;
                }
            }
        }

        return this;
    }

    public MuscleGroupOptions Build(Section section)
    {
        if (Context?.User != null)
        {
            if (MuscleGroups.Any())
            {
                Logs.AppendLog(Context.User, $"Muscle groups for {section}:{Environment.NewLine}{string.Join(", ", MuscleGroups)}");
            }

            if (MuscleTargetsRDA.Any())
            {
                Logs.AppendLog(Context.User, $"Muscle targets RDA for {section}:{Environment.NewLine}{string.Join(Environment.NewLine, MuscleTargetsRDA)}");
            }

            if (MuscleTargetsTUL.Any())
            {
                Logs.AppendLog(Context.User, $"Muscle targets TUL for {section}:{Environment.NewLine}{string.Join(Environment.NewLine, MuscleTargetsTUL)}");
            }
        }

        return new MuscleGroupOptions(MuscleGroups, MuscleTargetsRDA, MuscleTargetsTUL);
    }
}