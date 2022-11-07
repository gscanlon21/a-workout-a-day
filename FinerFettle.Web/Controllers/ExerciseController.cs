using FinerFettle.Web.Attributes.Response;
using FinerFettle.Web.Data;
using FinerFettle.Web.Models.Exercise;
using FinerFettle.Web.ViewModels.Exercise;
using FinerFettle.Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinerFettle.Web.Controllers;

[Route("exercises")]
public class ExerciseController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    public ExerciseController(CoreContext context) : base(context) { }

    [Route("all"), EnableRouteResponseCompression]
    public async Task<IActionResult> All([Bind("RecoveryMuscle,SportsFocus,OnlyWeights,OnlyUnilateral,IncludeMuscle,OnlyCore,EquipmentBinder,ShowFilteredOut,ExerciseType,MuscleContractions")] ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        viewModel.Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync();

        var queryBuilder = new ExerciseQueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All)
            .WithOrderBy(ExerciseQueryBuilder.OrderByEnum.Progression);

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.RecoveryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithRecoveryMuscle(viewModel.RecoveryMuscle.Value);
        }

        if (!viewModel.ShowFilteredOut)
        {
            if (viewModel.EquipmentIds != null)
            {
                queryBuilder = queryBuilder.WithEquipment(viewModel.EquipmentIds);
            }

            if (viewModel.OnlyUnilateral.HasValue)
            {
                queryBuilder = queryBuilder.IsUnilateral(viewModel.OnlyUnilateral == Models.NoYes.Yes);
            }

            if (viewModel.IncludeMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithIncludeMuscle(viewModel.IncludeMuscle);
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                queryBuilder = queryBuilder.WithOnlyWeights(viewModel.OnlyWeights.Value != Models.NoYes.No);
            }

            if (viewModel.OnlyCore.HasValue)
            {
                queryBuilder = queryBuilder.WithIncludeBonus(viewModel.OnlyCore.Value == Models.NoYes.No);
            }

            if (viewModel.ExerciseType.HasValue)
            {
                queryBuilder = queryBuilder.WithExerciseType(viewModel.ExerciseType.Value);
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleContractions(viewModel.MuscleContractions.Value);
            }
        }

        var allExercises = (await queryBuilder.Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        if (viewModel.ShowFilteredOut)
        {
            if (viewModel.OnlyCore.HasValue)
            {
                var temp = Filters.FilterIncludeBonus(allExercises.AsQueryable(), viewModel.OnlyCore.Value == Models.NoYes.No);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.OnlyUnilateral.HasValue)
            {
                var temp = Filters.FilterIsUnilateral(allExercises.AsQueryable(), viewModel.OnlyUnilateral.Value == Models.NoYes.Yes);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.IncludeMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.IncludeMuscle, include: true);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.EquipmentIds != null)
            {
                var temp = Filters.FilterEquipmentIds(allExercises.AsQueryable(), viewModel.EquipmentIds);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                var temp = Filters.FilterOnlyWeights(allExercises.AsQueryable(), viewModel.OnlyWeights.Value != Models.NoYes.No);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.ExerciseType.HasValue)
            {
                var temp = Filters.FilterExerciseType(allExercises.AsQueryable(), viewModel.ExerciseType);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                var temp = Filters.FilterMuscleContractions(allExercises.AsQueryable(), viewModel.MuscleContractions);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Other;
                    }
                });
            }
        }

        viewModel.Exercises = allExercises;

        return View(viewModel);
    }

    [Route("check")]
    public async Task<IActionResult> Check()
    {
        var allExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var strengthExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All)
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithExerciseType(ExerciseType.Stability | ExerciseType.Strength)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var recoveryExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All)
            .WithRecoveryMuscle(MuscleGroups.All)
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var warmupCooldownExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: true)
            .WithMuscleGroups(MuscleGroups.All)
            .WithExerciseType(ExerciseType.WarmupCooldown | ExerciseType.Cardio)
            .WithPrefersWeights(false)
            .CapAtProficiency(true)
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

        var missing100PProgressionRange = allExercises
            .Where(e => e.ExerciseVariation.IsBonus == false)
            .Where(e => e.Variation.DisabledReason == null)
            .GroupBy(e => e.Exercise.Name)
            .Where(e => e.Min(i => i.ExerciseVariation.Progression.GetMinOrDefault) > 0
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

        var missingProficiencyStrength = strengthExercises
            .Where(e => e.Variation.Intensities.All(p =>
                p.IntensityLevel != IntensityLevel.Maintain
                && p.IntensityLevel != IntensityLevel.Obtain
                && p.IntensityLevel != IntensityLevel.Gain
                //&& p.IntensityLevel != IntensityLevel.Endurance
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
