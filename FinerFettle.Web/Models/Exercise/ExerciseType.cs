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
        public IEnumerator<ExerciseRotaion> GetEnumerator()
        {
            yield return new ExerciseRotaion(ExerciseType.Strength, MuscleGroupings.UpperBody);
            yield return new ExerciseRotaion(ExerciseType.Strength, MuscleGroupings.MidBody);
            yield return new ExerciseRotaion(ExerciseType.Strength, MuscleGroupings.LowerBody);
            yield return new ExerciseRotaion(ExerciseType.Cardio, null);
            yield return new ExerciseRotaion(ExerciseType.Stability | ExerciseType.Flexibility, MuscleGroupings.All);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
