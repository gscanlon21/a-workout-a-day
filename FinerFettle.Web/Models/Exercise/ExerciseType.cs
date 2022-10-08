using System.Collections;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// Cardio/Strength/Stability/Flexibility.
    /// </summary>
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
        Cardio = 1 << 0, // 1

        /// <summary>
        /// Weight or resistance training. 
        /// Anerobic.
        /// </summary>
        Strength = 1 << 1, // 2

        /// <summary>
        /// Muscle control
        /// </summary>
        Stability = 1 << 2, // 4

        /// <summary>
        /// Muscle range of motion and movement. Most stretches are included in this.
        /// </summary>
        Flexibility = 1 << 3, // 8
    }

    /// <summary>
    /// Frequency of strengthing days
    /// </summary>
    public enum StrengtheningPreference
    {
        /// <summary>
        /// Strength exercises are always full-body exercises.
        /// </summary>
        Maintain = 0,

        /// <summary>
        /// Strength exercises rotate between upper body, mid body, and lower body.
        /// </summary>
        Obtain = 1,

        /// <summary>
        /// Strength exercises alternate between upper body and mid/lower body.
        /// </summary>
        Gain = 2,

        /// <summary>
        /// Endurance exercises work the full-body every day..
        /// </summary>
        Endurance = 3,

        /// <summary>
        /// Recovery exercises work the full-body every day..
        /// </summary>
        Recovery = 4
    }

    /// <summary>
    /// The ~weekly routine of exercise types for each strengthing preference.
    /// </summary>
    public class ExerciseTypeGroups : IEnumerable<ExerciseRotaion>
    {
        private readonly StrengtheningPreference StrengtheningPreference;

        public ExerciseTypeGroups(StrengtheningPreference preference)
        {
            StrengtheningPreference = preference;
        }

        public IEnumerator<ExerciseRotaion> GetEnumerator()
        {
            yield return new ExerciseRotaion(1, ExerciseType.Strength, StrengtheningPreference switch
            {
                StrengtheningPreference.Maintain => MuscleGroups.All,
                StrengtheningPreference.Obtain => MuscleGroups.UpperBody,
                StrengtheningPreference.Gain => MuscleGroups.UpperBody,
                StrengtheningPreference.Endurance => MuscleGroups.All,
                _ => MuscleGroups.All
            });

            if (StrengtheningPreference == StrengtheningPreference.Obtain || StrengtheningPreference == StrengtheningPreference.Gain)
            {
                yield return new ExerciseRotaion(2, ExerciseType.Strength, StrengtheningPreference switch
                {
                    StrengtheningPreference.Obtain => MuscleGroups.LowerBody,
                    StrengtheningPreference.Gain => MuscleGroups.LowerBody,
                    _ => MuscleGroups.All
                });
            } 

            yield return new ExerciseRotaion(3, ExerciseType.Cardio, MuscleGroups.All);
            yield return new ExerciseRotaion(4, ExerciseType.Strength, StrengtheningPreference switch
            {
                StrengtheningPreference.Maintain => MuscleGroups.All,
                StrengtheningPreference.Obtain => MuscleGroups.All,
                StrengtheningPreference.Gain => MuscleGroups.UpperBody,
                StrengtheningPreference.Endurance => MuscleGroups.All,
                _ => MuscleGroups.All
            });

            if (StrengtheningPreference == StrengtheningPreference.Gain)
            {
                yield return new ExerciseRotaion(5, ExerciseType.Strength, StrengtheningPreference switch
                {
                    StrengtheningPreference.Gain => MuscleGroups.LowerBody,
                    _ => MuscleGroups.All
                });
            }

            if (StrengtheningPreference != StrengtheningPreference.Endurance)
            {
                yield return new ExerciseRotaion(6, ExerciseType.Stability, MuscleGroups.All);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
