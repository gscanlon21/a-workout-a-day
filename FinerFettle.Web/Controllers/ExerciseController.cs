﻿using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.Models.Newsletter;
using FinerFettle.Web.Models.User;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json.Linq;
using NuGet.Common;

namespace FinerFettle.Web.Controllers
{
    [Route("exercises")]
    public class ExerciseController : BaseController
    {
        /// <summary>
        /// The name of the controller for routing purposes
        /// </summary>
        public const string Name = "Exercise";

        public ExerciseController(CoreContext context) : base(context) { }

        [Route("all")]
        public IActionResult All([Bind("RecoveryMuscle,SportsFocus")] ExercisesViewModel? viewModel = null)
        {
            viewModel ??= new ExercisesViewModel();

            var queryBuilder = new ExerciseQueryBuilder(_context)
                .WithMuscleGroups(MuscleGroups.All)
                .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.Progression);

            if (viewModel.SportsFocus.HasValue)
            {
                queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
            }

            if (viewModel.RecoveryMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithRecoveryMuscle(viewModel.RecoveryMuscle.Value, include: true);
            }
            else
            {
                // Otherwise exlude recovery tracks
                queryBuilder = queryBuilder.WithRecoveryMuscle(MuscleGroups.None);
            }

            var allExercises = queryBuilder.Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            viewModel.Exercises = allExercises;

            return View(viewModel);
        }

        [Route("check")]
        public IActionResult Check()
        {
            var allExercises = new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
                .WithMuscleGroups(MuscleGroups.All)
                .WithRecoveryMuscle(MuscleGroups.None)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var recoveryExercises = new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
                .WithMuscleGroups(MuscleGroups.All)
                .WithRecoveryMuscle(MuscleGroups.All, include: true)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var warmupCooldownExercises = new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
                .WithMuscleGroups(MuscleGroups.All)
                .WithExerciseType(ExerciseType.Flexibility | ExerciseType.Cardio)
                .WithPrefersWeights(false)
                .CapAtProficiency(true)
                .Query()
                .Select(r => new ExerciseViewModel(r, ExerciseActivityLevel.Main))
                .ToList();

            var missingExercises = _context.Variations
                .IgnoreQueryFilters()
                .Where(v => v.DisabledReason == null)
                // Left outer join
                .GroupJoin(_context.ExerciseVariations,
                    o => o.Id,
                    i => i.Variation.Id,
                    (o, i) => new { Variation = o, ExerciseVariations = i })
                .SelectMany(
                    oi => oi.ExerciseVariations.DefaultIfEmpty(),
                    (o, i) => new { o.Variation, ExerciseVariation = i })
                .Where(v => v.ExerciseVariation == null)
                .Select(v => v.Variation.Name)
                .ToList();

            var missing100PProgressionRange = allExercises.GroupBy(e => e.Exercise.Name)
                .Where(e =>
                    e.Min(i => i.ExerciseVariation.Progression.GetMinOrDefault) > 0
                || e.Max(i => i.ExerciseVariation.Progression.GetMaxOrDefault) < 100)
                .Select(e => e.Key)
                .ToList();

            var emptyDisabledString = allExercises
                .Where(e => e.Exercise.DisabledReason == string.Empty || e.Variation.DisabledReason == string.Empty)
                .Select(e => e.Exercise.Name)
                .ToList();

            var missingRepRange = allExercises
                .Where(e => e.Variation.Intensities.Any(p => (p.Proficiency.MinReps != null && p.Proficiency.MaxReps == null) || (p.Proficiency.MinReps == null && p.Proficiency.MaxReps != null)))
                .Select(e => e.Variation.Name)
                .ToList();

            var missingProficiencyStrength = allExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.Maintain
                    && p.IntensityLevel != IntensityLevel.Obtain
                    && p.IntensityLevel != IntensityLevel.Gain
                    && p.IntensityLevel != IntensityLevel.Endurance
                ))
                .Select(e => e.Variation.Name)
                .ToList();

            var missingProficiencyRecovery = recoveryExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.Recovery
                ))
                .Select(e => e.Variation.Name)
                .ToList();

            var missingProficiencyWarmupCooldown = warmupCooldownExercises
                .Where(e => e.Variation.Intensities.All(p =>
                    p.IntensityLevel != IntensityLevel.WarmupCooldown
                ))
                .Select(e => e.Variation.Name)
                .ToList();

            var viewModel = new CheckViewModel()
            {
                Missing100PProgressionRange = missing100PProgressionRange,
                MissingProficiencyStrength = missingProficiencyStrength,
                MissingProficiencyRecovery = missingProficiencyRecovery,
                MissingProficiencyWarmupCooldown = missingProficiencyWarmupCooldown,
                MissingRepRange = missingRepRange,
                EmptyDisabledString = emptyDisabledString,
                MissingExercises = missingExercises
            };

            return View(viewModel);
        }
    }
}
