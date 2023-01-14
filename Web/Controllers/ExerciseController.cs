using Web.Code.Attributes.Response;
using Web.Data;
using Web.Models.Exercise;
using Web.ViewModels.Exercise;
using Web.ViewModels.Newsletter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Data.QueryBuilder;
using Web.Entities.User;

namespace Web.Controllers;

[Route("exercises")]
public class ExerciseController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    public ExerciseController(CoreContext context) : base(context) { }

    [Route("all"), EnableRouteResponseCompression]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        viewModel.Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync();

        var queryBuilder = new ExerciseQueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
            })
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
                queryBuilder = queryBuilder.WithMuscleGroups(viewModel.IncludeMuscle.Value, x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
                });
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                queryBuilder = queryBuilder.WithOnlyWeights(viewModel.OnlyWeights.Value == Models.NoYes.Yes);
            }

            if (viewModel.OnlyAntiGravity.HasValue)
            {
                queryBuilder = queryBuilder.WithAntiGravity(viewModel.OnlyAntiGravity.Value == Models.NoYes.Yes);
            }

            if (viewModel.Bonus.HasValue)
            {
                queryBuilder = queryBuilder.WithBonus(viewModel.Bonus.Value, x => x.OnlyBonus = true);
            }

            if (viewModel.ExerciseType.HasValue)
            {
                queryBuilder = queryBuilder.WithExerciseType(viewModel.ExerciseType.Value);
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleContractions(viewModel.MuscleContractions.Value);
            }

            if (viewModel.MovementPatterns.HasValue)
            {
                queryBuilder = queryBuilder.WithMovementPatterns(viewModel.MovementPatterns.Value);
            }

            if (viewModel.MuscleMovement.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleMovement(viewModel.MuscleMovement.Value);
            }
        }

        var allExercises = (await queryBuilder.Build().Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        if (viewModel.ShowFilteredOut)
        {
            if (viewModel.Bonus.HasValue)
            {
                var temp = Filters.FilterIncludeBonus(allExercises.AsQueryable(), viewModel.Bonus.Value, onlyBonus: true);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.OnlyUnilateral.HasValue)
            {
                var temp = Filters.FilterIsUnilateral(allExercises.AsQueryable(), viewModel.OnlyUnilateral.Value == Models.NoYes.Yes);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.IncludeMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.IncludeMuscle, include: true, muscleTarget: vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.EquipmentIds != null)
            {
                var temp = Filters.FilterEquipmentIds(allExercises.AsQueryable(), viewModel.EquipmentIds);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                var temp = Filters.FilterOnlyWeights(allExercises.AsQueryable(), viewModel.OnlyWeights.Value == Models.NoYes.Yes);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.OnlyAntiGravity.HasValue)
            {
                var temp = Filters.FilterAntiGravity(allExercises.AsQueryable(), viewModel.OnlyAntiGravity.Value == Models.NoYes.Yes);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.ExerciseType.HasValue)
            {
                var temp = Filters.FilterExerciseType(allExercises.AsQueryable(), viewModel.ExerciseType);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MovementPatterns.HasValue)
            {
                var temp = Filters.FilterMovementPattern(allExercises.AsQueryable(), viewModel.MovementPatterns);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                var temp = Filters.FilterMuscleContractions(allExercises.AsQueryable(), viewModel.MuscleContractions);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
                    }
                });
            }

            if (viewModel.MuscleMovement.HasValue)
            {
                var temp = Filters.FilterMuscleMovement(allExercises.AsQueryable(), viewModel.MuscleMovement);
                allExercises.ForEach(e => {
                    if (!temp.Contains(e))
                    {
                        e.Theme = ExerciseTheme.Extra;
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
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var strengthExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithRecoveryMuscle(MuscleGroups.None)
            .WithExerciseType(ExerciseType.Main)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var recoveryExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithRecoveryMuscle(MuscleGroups.All)
            .Build()
            .Query())
            .Select(r => new ExerciseViewModel(r, ExerciseTheme.Main))
            .ToList();

        var warmupCooldownExercises = (await new ExerciseQueryBuilder(_context, ignoreGlobalQueryFilters: false)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StabilityMuscles | vm.Variation.StretchMuscles | vm.Variation.StrengthMuscles;
            })
            .WithExerciseType(ExerciseType.WarmupCooldown)
            .WithOnlyWeights(false)
            .WithProficency(x => {
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
            .Where(e => e.ExerciseVariation.Bonus == Models.User.Bonus.None)
            .Where(e => e.Variation.DisabledReason == null)
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
            .Where(e => e.Variation.MuscleMovement != MuscleMovement.Pylometric)
            .Where(e => e.Variation.StabilityMuscles != MuscleGroups.None)
            .Select(e => e.Variation.Name)
            .ToList();

        var missingRepRange = allExercises
            .Where(e => e.Variation.Intensities.Any(p => (p.Proficiency.MinReps != null && p.Proficiency.MaxReps == null) || (p.Proficiency.MinReps == null && p.Proficiency.MaxReps != null)))
            .Select(e => e.Variation.Name)
            .ToList();

        var strengthIntensities = new List<IntensityLevel>() {
            IntensityLevel.Maintain,
            IntensityLevel.Obtain,
            IntensityLevel.Gain,
            IntensityLevel.Endurance
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
