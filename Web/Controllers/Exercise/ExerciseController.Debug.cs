using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data.Query;
using Web.Entities.User;
using Web.Models.Exercise;
using Web.ViewModels.Exercise;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Exercise;

public partial class ExerciseController
{
    [Route("debug")]
    public async Task<IActionResult> Check()
    {
        var allExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
        .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var strengthExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            //.WithExerciseFocus(ExerciseFocus.Strengthening)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var recoveryExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var warmupCooldownExercises = (await new QueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            //.WithExerciseFocus(ExerciseFocus.Flexibility)
            .WithOnlyWeights(false)
            .WithProficency(x =>
            {
                x.DoCapAtProficiency = true;
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
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

        var progressionRange = Enumerable.Range(UserExercise.MinUserProgression, UserExercise.MaxUserProgression - UserExercise.MinUserProgression);
        var missing100ProgressionRange = allExercises
            .Where(e => e.Variation.DisabledReason == null && e.ExerciseVariation.DisabledReason == null)
            .GroupBy(e => e.Exercise.Name)
            .Where(g => !progressionRange.All(p => g.Any(e => p >= e.ExerciseVariation.Progression.GetMinOrDefault && p < e.ExerciseVariation.Progression.GetMaxOrDefault)))
            .Select(e => e.Key)
            .ToList();

        var emptyDisabledString = allExercises
            .Where(e => e.Exercise.DisabledReason == string.Empty || e.Variation.DisabledReason == string.Empty)
            .Select(e => e.Exercise.Name)
            .ToList();

        // The secondary muscles of a stretch are too hard to nail down...
        var stretchHasStability = warmupCooldownExercises
            .Where(e => e.Variation.MuscleMovement != MuscleMovement.Plyometric)
            .Where(e => e.Variation.StabilityMuscles != MuscleGroups.None)
            .Select(e => e.Variation.Name)
            .ToList();

        var missingRepRange = allExercises
            .Where(e => e.Variation.Intensities.Any(p => p.Proficiency.MinReps != null && p.Proficiency.MaxReps == null || p.Proficiency.MinReps == null && p.Proficiency.MaxReps != null))
            .Select(e => e.Variation.Name)
            .ToList();

        var strengthIntensities = new List<IntensityLevel>() {
            IntensityLevel.Endurance,
            IntensityLevel.Light,
            IntensityLevel.Medium,
            IntensityLevel.Heavy
        };
        var missingProficiencyStrength = strengthExercises
            .Where(e => e.Variation.Intensities
                .IntersectBy(strengthIntensities, i => i.IntensityLevel)
                .Count() < strengthIntensities.Count
            )
            .Select(e => e.Variation.Name)
            .ToList();

        var missingProficiencyRecovery = recoveryExercises
            .Where(e => e.Variation.Intensities.All(p =>
                p.IntensityLevel != IntensityLevel.Recovery
            ))
            .Select(e => e.Variation.Name)
            .ToList();

        var warmupCooldownIntensities = new List<IntensityLevel>() {
            IntensityLevel.Warmup,
            IntensityLevel.Cooldown
        };
        var missingProficiencyWarmupCooldown = warmupCooldownExercises
            .Where(e => e.Variation.Intensities
                .IntersectBy(warmupCooldownIntensities, i => i.IntensityLevel)
                .Count() < warmupCooldownIntensities.Count
            )
            .Select(e => e.Variation.Name)
            .ToList();

        var viewModel = new CheckViewModel()
        {
            StretchHasStability = stretchHasStability,
            Missing100PProgressionRange = missing100ProgressionRange,
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
