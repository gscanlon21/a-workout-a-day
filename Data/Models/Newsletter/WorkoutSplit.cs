using Core.Dtos.Newsletter;
using Core.Dtos.User;
using Core.Models.Exercise;
using Core.Models.User;
using Data.Entities.Newsletter;
using Data.Entities.User;
using System.Collections;
using Web.Code;

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
public class WorkoutSplit : IEnumerable<WorkoutRotationDto>, IEnumerator<WorkoutRotationDto>
{
    private readonly Frequency Frequency;

    private readonly WorkoutRotationDto[] _Rotations;

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
            Frequency.None => [],
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
            Frequency.None => [],
            Frequency.Custom => (user.UserFrequencies.Select(f => f.Rotation.AsType<WorkoutRotationDto, WorkoutRotation>()!).OrderBy(r => r.Id).NullIfEmpty() ?? GetFullBody2DayRotation()).ToArray(),
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

    public WorkoutRotationDto Current
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

    public IEnumerator<WorkoutRotationDto> GetEnumerator()
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
    private static IEnumerable<WorkoutRotationDto> GetOffDayStretchingRotation(User? user = null)
    {
        var mobilityMuscleGroups = UserMuscleMobilityDto.MuscleTargets
            .ToDictionary(kv => kv.Key, kv => user?.UserMuscleMobilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)
            .Where(d => d.Value > 0).Select(kv => kv.Key);

        var flexibilityMuscleGroups = UserMuscleFlexibilityDto.MuscleTargets
            .ToDictionary(kv => kv.Key, kv => user?.UserMuscleFlexibilities.SingleOrDefault(umm => umm.MuscleGroup == kv.Key)?.Count ?? kv.Value)
            .Where(d => d.Value > 0).Select(kv => kv.Key);

        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. mobilityMuscleGroups, .. flexibilityMuscleGroups],
            MovementPatterns = MovementPattern.None
        };
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    public static IEnumerable<WorkoutRotationDto> GetFullBody2DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperLower()],
            MovementPatterns = MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.HipExtension | MovementPattern.Carry
        };

        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperLower()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.KneeFlexion | MovementPattern.Rotation
        };
    }

    /// <summary>
    /// An implementation of the Upper/Lower/Full-Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotationDto> GetUpperLowerFullBody3DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.HipExtension | MovementPattern.KneeFlexion
        };

        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Upper()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.VerticalPush | MovementPattern.VerticalPull
        };

        yield return new WorkoutRotationDto
        {
            Id = 3,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperLower()],
            MovementPatterns = MovementPattern.Carry | MovementPattern.Rotation
        };
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotationDto> GetPushPullLeg3DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPull()],
            MovementPatterns = MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry
        };
        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPush()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation
        };
        yield return new WorkoutRotationDto
        {
            Id = 3,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.HipExtension | MovementPattern.KneeFlexion
        };
    }

    /// <summary>
    /// An implementation of the Upper/Lower workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotationDto> GetUpperLower4DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Upper()],
            MovementPatterns = MovementPattern.VerticalPush | MovementPattern.VerticalPull
        };
        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.HipExtension | MovementPattern.Carry
        };
        yield return new WorkoutRotationDto
        {
            Id = 3,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Upper()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.HorizontalPull
        };
        yield return new WorkoutRotationDto
        {
            Id = 4,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.KneeFlexion | MovementPattern.Rotation
        };
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs/Full-Body workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotationDto> GetPushPullLegsFullBody4DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPull()],
            MovementPatterns = MovementPattern.HorizontalPull | MovementPattern.VerticalPull
        };
        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPush()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.VerticalPush
        };
        yield return new WorkoutRotationDto
        {
            Id = 3,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.HipExtension | MovementPattern.KneeFlexion
        };
        yield return new WorkoutRotationDto
        {
            Id = 4,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperLower()],
            MovementPatterns = MovementPattern.Carry | MovementPattern.Rotation
        };
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs/Upper/Lower workout split.
    /// </summary>
    private static IEnumerable<WorkoutRotationDto> GetPushPullLegsUpperLower5DayRotation()
    {
        yield return new WorkoutRotationDto
        {
            Id = 1,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPull()],
            MovementPatterns = MovementPattern.HorizontalPull | MovementPattern.VerticalPull
        };
        yield return new WorkoutRotationDto
        {
            Id = 2,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.UpperPush()],
            MovementPatterns = MovementPattern.HorizontalPush | MovementPattern.VerticalPush
        };
        yield return new WorkoutRotationDto
        {
            Id = 3,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.HipExtension
        };
        yield return new WorkoutRotationDto
        {
            Id = 4,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Upper()],
            MovementPatterns = MovementPattern.Carry | MovementPattern.Rotation
        };
        yield return new WorkoutRotationDto
        {
            Id = 5,
            MuscleGroups = [.. MuscleGroupExtensions.Core(), .. MuscleGroupExtensions.Lower()],
            MovementPatterns = MovementPattern.Squat | MovementPattern.Lunge
        };
    }
}
