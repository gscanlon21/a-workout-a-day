using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace FinerFettle.Web.Models.Exercise
{
    [Flags]
    public enum ExerciseType
    {
        /// <summary>
        /// Rest
        /// </summary>
        None = 0,

        /// <summary>
        /// Cardio. 
        /// Aerobic.
        /// </summary>
        Cardio = 1 << 0,

        /// <summary>
        /// Weight or resistance training. 
        /// Anerobic.
        /// </summary>
        Strength = 1 << 1,

        /// <summary>
        /// Muscle control
        /// </summary>
        Stability = 1 << 2,

        /// <summary>
        /// Muscle range of motion and movement. Most stretches are included in this.
        /// </summary>
        Flexibility = 1 << 3,
    }

    public class ExerciseTypeGroups : IEnumerable<ExerciseRotaion>
    {
        public const ExerciseType StretchStrength =  ExerciseType.Strength;
        public const ExerciseType StretchAerobic = ExerciseType.Cardio;
        public const ExerciseType StabilityFlexibility = ExerciseType.Stability | ExerciseType.Flexibility;

        public IEnumerator<ExerciseRotaion> GetEnumerator()
        {
            yield return new ExerciseRotaion(StretchStrength, MuscleGroupings.UpperBodyPush);
            yield return new ExerciseRotaion(StretchStrength, MuscleGroupings.UpperBodyPull);
            yield return new ExerciseRotaion(StretchStrength, MuscleGroupings.Core);
            yield return new ExerciseRotaion(StretchStrength, MuscleGroupings.MidBody);
            yield return new ExerciseRotaion(StretchStrength, MuscleGroupings.LowerBody);
            yield return new ExerciseRotaion(StretchAerobic, null);
            yield return new ExerciseRotaion(StabilityFlexibility, MuscleGroupings.All);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
