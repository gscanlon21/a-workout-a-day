﻿using FinerFettle.Web.Models.Exercise;
using System.ComponentModel.DataAnnotations;

namespace FinerFettle.Web.ViewModels.Newsletter
{
    // TODO: [DebuggerDisplay] attributes
    public class ExerciseViewModel
    {
        public ExerciseViewModel(Models.User.User? user, IList<Equipment> equipment, Variation exercise, Intensity intensity, MuscleGroups muscles, ExerciseType type, Models.User.ExerciseUserProgression? userProgression)
        {
            Equipment = equipment;
            User = user;
            Exercise = exercise;
            Intensity = intensity;
            Muscles = muscles;
            ExerciseType = type;
            UserProgression = userProgression;

            if (intensity.IntensityLevel == IntensityLevel.Main)
            {
                ActivityLevel = ExerciseActivityLevel.Main;
            } 
            else if (intensity.IntensityLevel == IntensityLevel.Stretch)
            {
                if (exercise.MuscleContractions == MuscleContractions.Isometric)
                {
                    // Choose static stretches for cooldown exercises
                    ActivityLevel = ExerciseActivityLevel.Cooldown;
                }
                else
                {
                    // Choose dynamic stretches for warmup exercises.
                    // Warmup exercises may include short isometric holds between reps.
                    ActivityLevel = ExerciseActivityLevel.Warmup;
                }
            }
        }

        public ExerciseActivityLevel ActivityLevel { get; }

        public MuscleGroups Muscles { get; init; }

        public ExerciseType ExerciseType { get; init; }

        public Models.User.User? User { get; init; }

        public Models.User.ExerciseUserProgression? UserProgression { get; set; }

        public bool HasLowerProgressionVariation { get; set; }
        public bool HasHigherProgressionVariation { get; set; }

        public Variation Exercise { get; init; }

        [UIHint(nameof(Intensity))]
        public Intensity Intensity { get; init; }

        public IList<Equipment> Equipment { get; init; }
    }
}
