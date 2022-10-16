using FinerFettle.Web.Models.User;
using System.Collections;

namespace FinerFettle.Web.Models.Exercise
{
    /// <summary>
    /// The ~weekly routine of exercise types for each strengthing preference.
    /// </summary>
    public class ExerciseTypeGroups : IEnumerable<ExerciseRotation>
    {
        private readonly StrengtheningPreference StrengtheningPreference;

        public ExerciseTypeGroups(StrengtheningPreference preference)
        {
            StrengtheningPreference = preference;
        }

        public IEnumerator<ExerciseRotation> GetEnumerator()
        {
            yield return new ExerciseRotation(1, ExerciseType.Strength, StrengtheningPreference switch
            {
                StrengtheningPreference.Maintain => MuscleGroups.All,
                StrengtheningPreference.Obtain => MuscleGroups.UpperBody,
                StrengtheningPreference.Gain => MuscleGroups.UpperBody,
                StrengtheningPreference.Endurance => MuscleGroups.UpperBody,
                _ => MuscleGroups.All
            });

            if (StrengtheningPreference == StrengtheningPreference.Obtain
                || StrengtheningPreference == StrengtheningPreference.Gain
                || StrengtheningPreference == StrengtheningPreference.Endurance)
            {
                yield return new ExerciseRotation(2, ExerciseType.Strength, StrengtheningPreference switch
                {
                    StrengtheningPreference.Obtain => MuscleGroups.LowerBody,
                    StrengtheningPreference.Gain => MuscleGroups.LowerBody,
                    StrengtheningPreference.Endurance => MuscleGroups.LowerBody,
                    _ => MuscleGroups.All
                });
            }

            yield return new ExerciseRotation(3, ExerciseType.Cardio, MuscleGroups.All);

            if (StrengtheningPreference != StrengtheningPreference.Endurance)
            {
                yield return new ExerciseRotation(4, ExerciseType.Strength, StrengtheningPreference switch
                {
                    StrengtheningPreference.Maintain => MuscleGroups.All,
                    StrengtheningPreference.Obtain => MuscleGroups.All,
                    StrengtheningPreference.Gain => MuscleGroups.UpperBody,
                    _ => MuscleGroups.All
                });

                if (StrengtheningPreference == StrengtheningPreference.Gain)
                {
                    yield return new ExerciseRotation(5, ExerciseType.Strength, StrengtheningPreference switch
                    {
                        StrengtheningPreference.Gain => MuscleGroups.LowerBody,
                        _ => MuscleGroups.All
                    });
                }

                yield return new ExerciseRotation(6, ExerciseType.Stability, MuscleGroups.All);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
