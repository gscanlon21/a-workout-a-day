using Core.Code.Extensions;
using Core.Models.Exercise;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using System.Collections;

namespace Data.Models.Newsletter;

/// <summary>
/// The workout rotations of each workout split.
/// 
/// Implementation Notes: 
/// This type implements both IEnumerable and IEnumerator and should only be iterated over once for each instance.
/// This type does not ensure a new enumerable is created for every call to GetEnumerator().
/// 
/// This type does not need to be disposed.
/// 
/// This type does support Reset() for multiple passes.
/// </summary>
public class WorkoutSplit : IEnumerable<WorkoutRotation>, IEnumerator<WorkoutRotation>
{
    private readonly Frequency Frequency;

    private readonly WorkoutRotation[] _Rotations;

    /// <summary>
    /// Enumerators are positioned before the first element until the first MoveNext() call.
    /// </summary>
    private readonly int _StartingIndex = -1;

    /// <summary>
    /// Enumerators are positioned before the first element until the first MoveNext() call.
    /// </summary>
    private int _Position = -1;

    private int _Iterations = 0;

    /// <summary>
    /// Creates an instance that starts at the default newsletter rotation.
    /// </summary>
    /// <param name="frequency"></param>
    public WorkoutSplit(Frequency frequency)
    {
        Frequency = frequency;

        _Rotations = Frequency switch
        {
            Frequency.None => Array.Empty<WorkoutRotation>(),
            Frequency.Custom => throw new NotSupportedException(),
            Frequency.FullBody2Day => GetFullBody2DayRotation().ToArray(),
            Frequency.PushPullLeg3Day => GetPushPullLeg3DayRotation().ToArray(),
            Frequency.UpperLowerBodySplit4Day => GetUpperLower4DayRotation().ToArray(),
            Frequency.UpperLowerFullBodySplit3Day => GetUpperLowerFullBody3DayRotation().ToArray(),
            Frequency.PushPullLegsFullBodySplit4Day => GetPushPullLegsFullBody4DayRotation().ToArray(),
            Frequency.PushPullLegsUpperLowerSplit5Day => GetPushPullLegsUpperLower5DayRotation().ToArray(),
            Frequency.OffDayStretches => GetOffDayStretchingRotation().ToArray(),
            _ => throw new NotImplementedException()
        };
    }

    /// <summary>
    /// Creates an instance that starts at the next newsletter rotation.
    /// </summary>
    public WorkoutSplit(Frequency frequency, User user, WorkoutRotation? previousRotation)
    {
        Frequency = frequency;

        if (previousRotation != null)
        {
            // -1 since the Ids start at one and -1 since enumerators are positioned before the first element until the first MoveNext() call.
            _StartingIndex = _Position = previousRotation.Id - 1;
        }

        _Rotations = Frequency switch
        {
            Frequency.None => Array.Empty<WorkoutRotation>(),
            Frequency.Custom => (user.UserFrequencies.Select(f => f.Rotation).OrderBy(r => r.Id).NullIfEmpty() ?? GetFullBody2DayRotation()).ToArray(),
            Frequency.FullBody2Day => GetFullBody2DayRotation().ToArray(),
            Frequency.PushPullLeg3Day => GetPushPullLeg3DayRotation().ToArray(),
            Frequency.UpperLowerBodySplit4Day => GetUpperLower4DayRotation().ToArray(),
            Frequency.UpperLowerFullBodySplit3Day => GetUpperLowerFullBody3DayRotation().ToArray(),
            Frequency.PushPullLegsFullBodySplit4Day => GetPushPullLegsFullBody4DayRotation().ToArray(),
            Frequency.PushPullLegsUpperLowerSplit5Day => GetPushPullLegsUpperLower5DayRotation().ToArray(),
            Frequency.OffDayStretches => GetOffDayStretchingRotation(user).ToArray(),
            _ => throw new NotImplementedException()
        };
    }

    object IEnumerator.Current => Current;

    public WorkoutRotation Current
    {
        get
        {
            try
            {
                return _Rotations[_Position];
            }
            catch (IndexOutOfRangeException)
            {
                throw new InvalidOperationException();
            }
        }
    }

    public bool MoveNext()
    {
        _Position++;
        _Iterations++;

        if (_Position >= _Rotations.Length)
        {
            _Position = 0;
        }

        return _Iterations <= _Rotations.Length;
    }

    public void Reset()
    {
        // Enumerators are positioned before the first element until the first MoveNext() call.
        _Position = _StartingIndex;
        _Iterations = 0;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public IEnumerator<WorkoutRotation> GetEnumerator()
    {
        if (_Iterations > 0)
        {
            throw new InvalidOperationException("The enumerator has already been exhausted.");
        }

        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Off-day mobility/stretching rotation.
    /// 
    /// We intersect the muscle groups with the user's StretchingMuscles.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetOffDayStretchingRotation(User? user = null)
    {
        var mobilityMuscleGroups = UserMuscleMobility.MuscleTargets
            .ToDictionary(kv => kv.Key, kv => user?.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)
            .Where(d => d.Value > 0).Aggregate(MuscleGroups.None, (curr, n) => curr | n.Key);

        var flexibilityMuscleGroups = UserMuscleFlexibility.MuscleTargets
            .ToDictionary(kv => kv.Key, kv => user?.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)
            .Where(d => d.Value > 0).Aggregate(MuscleGroups.None, (curr, n) => curr | n.Key);

        yield return new WorkoutRotation(1, mobilityMuscleGroups | flexibilityMuscleGroups, MovementPattern.None);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    public static IEnumerable<WorkoutRotation> GetFullBody2DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.UpperLower,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Squat | MovementPattern.HipExtension | MovementPattern.Rotation);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.Lunge | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetUpperLowerFullBody3DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Carry);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new WorkoutRotation(3,
            MuscleGroups.Core | MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.Lunge | MovementPattern.HipExtension);
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetPushPullLeg3DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new WorkoutRotation(3,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);
    }

    /// <summary>
    /// An implementation of the Upper/Lower Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetUpperLower4DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat);

        yield return new WorkoutRotation(3,
            MuscleGroups.Core | MuscleGroups.UpperBody,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.Carry);

        yield return new WorkoutRotation(4,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Lunge);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetPushPullLegsFullBody4DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new WorkoutRotation(3,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat);

        yield return new WorkoutRotation(4,
            MuscleGroups.Core | MuscleGroups.UpperLower,
            MovementPattern.HipExtension | MovementPattern.Lunge);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotation> GetPushPullLegsUpperLower5DayRotation()
    {
        yield return new WorkoutRotation(1,
            MuscleGroups.Core | MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry);

        yield return new WorkoutRotation(2,
            MuscleGroups.Core | MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new WorkoutRotation(3,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat);

        yield return new WorkoutRotation(4,
            MuscleGroups.Core | MuscleGroups.UpperBody,
            MovementPattern.None);

        yield return new WorkoutRotation(5,
            MuscleGroups.Core | MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Lunge);
    }
}
