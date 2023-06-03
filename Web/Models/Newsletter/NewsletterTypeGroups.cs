using System.Collections;
using Web.Code.Extensions;
using Web.Entities.Newsletter;
using Web.Models.Exercise;
using Web.Models.User;

namespace Web.Models.Newsletter;

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
public class NewsletterTypeGroups : IEnumerable<NewsletterRotation>, IEnumerator<NewsletterRotation>
{
    private readonly Frequency Frequency;

    private readonly NewsletterRotation[] _Rotations;

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
    public NewsletterTypeGroups(Frequency frequency) : this(frequency, previousRotation: null) { }

    /// <summary>
    /// Creates an instance that starts at the next newsletter rotation.
    /// </summary>
    public NewsletterTypeGroups(Frequency frequency, NewsletterRotation? previousRotation)
    {
        Frequency = frequency;

        if (previousRotation != null)
        {
            // -1 since the Ids start at one and -1 since enumerators are positioned before the first element until the first MoveNext() call.
            _Position = previousRotation.Id - 1;
            _StartingIndex = previousRotation.Id - 1;
        }

        _Rotations = Frequency switch
        {
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
    public NewsletterTypeGroups(Entities.User.User user, Frequency frequency, NewsletterRotation? previousRotation)
    {
        Frequency = frequency;

        if (previousRotation != null)
        {
            // -1 since the Ids start at one and -1 since enumerators are positioned before the first element until the first MoveNext() call.
            _Position = previousRotation.Id - 1;
            _StartingIndex = previousRotation.Id - 1;
        }

        _Rotations = Frequency switch
        {
            Frequency.Custom => (user.UserFrequencies.Select(f => f.Rotation).OrderBy(r => r.Id).NullIfEmpty() ?? GetCustomDefaultRotation()).ToArray(),
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

    object IEnumerator.Current => Current;

    public NewsletterRotation Current
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
    
    public IEnumerator<NewsletterRotation> GetEnumerator()
    {
        if (this._Iterations > 0)
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
    private static IEnumerable<NewsletterRotation> GetOffDayStretchingRotation()
    {
        yield return new NewsletterRotation(1, MuscleGroups.All, MovementPattern.None);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    public static IEnumerable<NewsletterRotation> GetCustomDefaultRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperLower,
            MovementPattern.None);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    public static IEnumerable<NewsletterRotation> GetFullBody2DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperLower,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.KneeFlexion | MovementPattern.HipExtension | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<NewsletterRotation> GetUpperLowerFullBody3DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion | MovementPattern.Carry);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);
        
        yield return new NewsletterRotation(3,
            MuscleGroups.UpperLower,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull | MovementPattern.KneeFlexion | MovementPattern.HipExtension);
    }

    /// <summary>
    /// An implementation of the Push/Pull/Legs workout split.
    /// </summary>
    private static IEnumerable<NewsletterRotation> GetPushPullLeg3DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull | MovementPattern.Carry);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush | MovementPattern.Rotation);

        yield return new NewsletterRotation(3,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.Squat | MovementPattern.Lunge);
    }

    /// <summary>
    /// An implementation of the Upper/Lower Body workout split.
    /// </summary>
    private static IEnumerable<NewsletterRotation> GetUpperLower4DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBody,
            MovementPattern.HorizontalPush | MovementPattern.HorizontalPull | MovementPattern.Rotation);

        yield return new NewsletterRotation(2,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion | MovementPattern.Carry);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBody,
            MovementPattern.VerticalPush | MovementPattern.VerticalPull);

        yield return new NewsletterRotation(4,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<NewsletterRotation> GetPushPullLegsFullBody4DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush);

        yield return new NewsletterRotation(2,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion);

        yield return new NewsletterRotation(3,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull);

        yield return new NewsletterRotation(4,
            MuscleGroups.UpperLower,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion | MovementPattern.Rotation | MovementPattern.Carry);
    }

    /// <summary>
    /// An implementation of the Full Body workout split.
    /// </summary>
    private static IEnumerable<NewsletterRotation> GetPushPullLegsUpperLower5DayRotation()
    {
        yield return new NewsletterRotation(1,
            MuscleGroups.UpperBodyPush,
            MovementPattern.HorizontalPush | MovementPattern.VerticalPush);

        yield return new NewsletterRotation(2,
            MuscleGroups.UpperBodyPull,
            MovementPattern.HorizontalPull | MovementPattern.VerticalPull);

        yield return new NewsletterRotation(3,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion);

        yield return new NewsletterRotation(4,
            MuscleGroups.UpperBody,
            MovementPattern.Carry | MovementPattern.Rotation);

        yield return new NewsletterRotation(5,
            MuscleGroups.LowerBody,
            MovementPattern.HipExtension | MovementPattern.KneeFlexion | MovementPattern.Carry | MovementPattern.Rotation);
    }
}
