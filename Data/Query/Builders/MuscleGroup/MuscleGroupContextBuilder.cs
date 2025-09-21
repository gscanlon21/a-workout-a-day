using Core.Dtos.Newsletter;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Query.Options;

namespace Data.Query.Builders.MuscleGroup;

public interface IMuscleGroupContextBuilderStep1 : IMuscleGroupBuilder
{
    IMuscleGroupContextBuilderStep2 WithMuscleTargets(Dictionary<MusculoskeletalSystem, int> muscleTargets);
    IMuscleGroupContextBuilderStep2 WithMuscleTargetsFromMuscleGroups(IDictionary<MusculoskeletalSystem, int>? workedMusclesDict = null);
}

public interface IMuscleGroupContextBuilderStep2 : IMuscleGroupBuilder
{
    IMuscleGroupBuilder AdjustMuscleTargets(bool adjustUp = true, bool adjustDown = true, bool adjustUpBuffer = true, bool adjustDownBuffer = false, IList<WorkoutRotationDto>? rotations = null);
}

/// <summary>
/// Step-builder pattern for muscle target options.
/// </summary>
public class MuscleGroupContextBuilder : IMuscleGroupBuilder, IMuscleGroupContextBuilderStep1, IMuscleGroupContextBuilderStep2
{
    private readonly WorkoutContext Context;

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    private IList<MusculoskeletalSystem> MuscleGroups;

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// </summary>
    public Dictionary<MusculoskeletalSystem, int> MuscleTargetsRDA = [];

    /// <summary>
    /// Filters variations to only those that target these muscle groups.
    /// <para>
    /// -1 means we don't choose any exercises that work this muscle. 
    /// 0 means we don't specifically target this muscle, but exercises working other muscles may still be picked.
    /// </para>
    /// </summary>
    public Dictionary<MusculoskeletalSystem, int> MuscleTargetsTUL = [];

    private MuscleGroupContextBuilder(WorkoutContext context, IList<MusculoskeletalSystem> muscleGroups)
    {
        MuscleGroups = muscleGroups;
        Context = context;
    }

    public static IMuscleGroupContextBuilderStep1 WithMuscleGroups(WorkoutContext context, IList<MusculoskeletalSystem> muscleGroups)
    {
        return new MuscleGroupContextBuilder(context, muscleGroups);
    }

    public IMuscleGroupContextBuilderStep2 WithMuscleTargets(Dictionary<MusculoskeletalSystem, int> muscleTargets)
    {
        MuscleTargetsRDA = muscleTargets;
        // Add a one exercise buffer to the TUL when passing in the target volume directly.
        MuscleTargetsTUL = muscleTargets.ToDictionary(mt => mt.Key, mt => mt.Value + ExerciseConsts.TargetVolumePerExercise);

        return this;
    }

    public IMuscleGroupContextBuilderStep2 WithMuscleTargetsFromMuscleGroups(IDictionary<MusculoskeletalSystem, int>? workedMusclesDict = null)
    {
        // Base 1 target for each targeted muscle group. If we've already worked this muscle, reduce the muscle target volume. Keep all muscle groups in our target dict so we exclude overworked muscles.
        MuscleTargetsRDA = UserMuscleStrength.MuscleTargets.Keys.ToDictionary(mt => mt, mt => (MuscleGroups.Any(mg => mt.HasFlag(mg)) ? ExerciseConsts.TargetVolumePerExercise : 0) - (workedMusclesDict?.TryGetValue(mt, out int workedAmt) ?? false ? workedAmt : 0));

        // Base 0 target for each targeted muscle group. If we've already worked this muscle, reduce the muscle target volume. Keep all muscle groups in our target dict so we exclude overworked muscles.
        MuscleTargetsTUL = UserMuscleStrength.MuscleTargets.Keys.ToDictionary(mt => mt, mt => workedMusclesDict?.TryGetValue(mt, out int workedAmt) ?? false ? -workedAmt : 0);

        return this;
    }

    /// <summary>
    /// Adjustments to the muscle groups to reduce muscle imbalances.
    /// Note: Don't change too much during deload weeks or they don't throw off the weekly muscle target tracking.
    /// </summary>
    public IMuscleGroupBuilder AdjustMuscleTargets(bool adjustUp = true, bool adjustDown = true, bool adjustUpBuffer = true, bool adjustDownBuffer = false, IList<WorkoutRotationDto>? rotations = null)
    {
        if (Context.WeeklyMusclesRDA != null)
        {
            foreach (var key in MuscleTargetsRDA.Keys.Where(key => Context.WeeklyMusclesRDA[key].HasValue))
            {
                // We want a buffer before excluding muscle groups to where we don't target the muscle group, but still allow exercises that target the muscle to be chosen.
                // Forearms, for example, are rarely something we want to target directly, since they are worked in many functional movements.
                if (adjustUpBuffer && int.IsNegative(Context.WeeklyMusclesRDA[key]!.Value))
                {
                    MuscleGroups.Remove(key);
                }

                // Always lower the RDA if the muscle doesn't need to be worked this week.
                if (adjustUp || int.IsNegative(Context.WeeklyMusclesRDA[key]!.Value))
                {
                    // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks. Reduce the scale when the user has less workouts in a week.
                    var workoutsTargetingMuscleGroupPerWeek = Math.Max(1d, rotations?.Count(r => r.MuscleGroupsWithCore.Contains(key)) ?? Context.User.SendDays.PopCount());
                    var target = (int)Math.Round(MuscleTargetsRDA[key] / workoutsTargetingMuscleGroupPerWeek);

                    // Cap the muscle targets, so we never get more than 3 accessory exercises a day for a specific muscle group.
                    MuscleTargetsRDA[key] = Math.Min(Context.WeeklyMusclesRDA[key]!.Value + target, ExerciseConsts.TargetVolumePerExercise * 2);
                }
            }
        }

        if (Context.WeeklyMusclesTUL != null && adjustDown)
        {
            foreach (var key in MuscleTargetsTUL.Keys.Where(key => Context.WeeklyMusclesTUL[key].HasValue))
            {
                // Adjust muscle targets based on the user's weekly muscle volume averages over the last several weeks. Reduce the scale when the user has less workouts in a week.
                var workoutsTargetingMuscleGroupPerWeek = Math.Max(1d, rotations?.Count(r => r.MuscleGroupsWithCore.Contains(key)) ?? Context.User.SendDays.PopCount());
                var adjustDownBufferAdjustment = adjustDownBuffer ? ExerciseConsts.TargetVolumePerExercise : 0; // Reduce TUL when we want to buffer down.
                var target = (int)Math.Round((MuscleTargetsTUL[key] - adjustDownBufferAdjustment) / workoutsTargetingMuscleGroupPerWeek);

                // Cap the muscle targets, so we never get more than 3 accessory exercises a day for a specific muscle group.
                MuscleTargetsTUL[key] = Math.Min(Context.WeeklyMusclesTUL[key]!.Value + target, ExerciseConsts.TargetVolumePerExercise * 2);
            }
        }

        return this;
    }

    public MuscleGroupOptions Build(Section section)
    {
        if (MuscleGroups.Any())
        {
            UserLogs.Log(Context.User, $"Muscle groups for {section}:{Environment.NewLine}{string.Join(", ", MuscleGroups)}");
        }

        if (MuscleTargetsRDA.Any())
        {
            UserLogs.Log(Context.User, $"Muscle targets RDA for {section}:{Environment.NewLine}{string.Join(Environment.NewLine, MuscleTargetsRDA)}");
        }

        if (MuscleTargetsTUL.Any())
        {
            UserLogs.Log(Context.User, $"Muscle targets TUL for {section}:{Environment.NewLine}{string.Join(Environment.NewLine, MuscleTargetsTUL)}");
        }

        return new MuscleGroupOptions(MuscleGroups, MuscleTargetsRDA, MuscleTargetsTUL);
    }
}