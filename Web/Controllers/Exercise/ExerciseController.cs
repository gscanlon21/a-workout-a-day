using Core.Debug;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.Attributes;
using Web.Code.Extensions;
using Web.Data;
using Web.Data.Query;
using Web.Models.Exercise;
using Web.ViewModels.Exercise;
using Web.ViewModels.Newsletter;

namespace Web.Controllers.Exercise;

[Route("exercise")]
public partial class ExerciseController : BaseController
{
    /// <summary>
    /// The name of the controller for routing purposes
    /// </summary>
    public const string Name = "Exercise";

    public ExerciseController(CoreContext context) : base(context) { }

    [Route("all"), ResponseCompression(Enabled = !Consts.IsDebug)]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        viewModel.Equipment = await _context.Equipment
                .Where(e => e.DisabledReason == null)
                .OrderBy(e => e.Name)
                .ToListAsync();

        var queryBuilder = new QueryBuilder(_context)
            .WithMuscleGroups(MuscleGroups.All, x =>
            {
                x.MuscleTarget = vm => vm.Variation.StrengthMuscles | vm.Variation.StretchMuscles | vm.Variation.StabilityMuscles;
            })
            .WithOrderBy(OrderBy.Progression);

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.Joints.HasValue)
        {
            queryBuilder = queryBuilder.WithJoints(viewModel.Joints.Value);
        }

        if (!viewModel.InvertFilters)
        {
            if (viewModel.EquipmentIds != null)
            {
                queryBuilder = queryBuilder.WithEquipment(viewModel.EquipmentIds);
            }

            if (viewModel.OnlyUnilateral.HasValue)
            {
                queryBuilder = queryBuilder.IsUnilateral(viewModel.OnlyUnilateral == Models.NoYes.Yes);
            }

            if (viewModel.StrengthMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleGroups(viewModel.StrengthMuscle.Value, x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StrengthMuscles;
                });
            }

            if (viewModel.StretchMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleGroups(viewModel.StretchMuscle.Value, x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StretchMuscles;
                });
            }

            if (viewModel.Joints.HasValue)
            {
                queryBuilder = queryBuilder.WithJoints(viewModel.Joints.Value);
            }

            if (viewModel.StabilityMuscle.HasValue)
            {
                queryBuilder = queryBuilder.WithMuscleGroups(viewModel.StabilityMuscle.Value, x =>
                {
                    x.MuscleTarget = vm => vm.Variation.StabilityMuscles;
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

            if (viewModel.ExerciseSection.HasValue)
            {
                queryBuilder = queryBuilder.WithExerciseSection(viewModel.ExerciseSection.Value);
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

        if (viewModel.InvertFilters)
        {
            if (viewModel.OnlyUnilateral.HasValue)
            {
                var temp = Filters.FilterIsUnilateral(allExercises.AsQueryable(), viewModel.OnlyUnilateral.Value == Models.NoYes.Yes);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.StrengthMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.StrengthMuscle, include: true, muscleTarget: vm => vm.Variation.StrengthMuscles);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.StabilityMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.StabilityMuscle, include: true, muscleTarget: vm => vm.Variation.StabilityMuscles);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.StretchMuscle.HasValue)
            {
                var temp = Filters.FilterMuscleGroup(allExercises.AsQueryable(), viewModel.StretchMuscle, include: true, muscleTarget: vm => vm.Variation.StretchMuscles);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.Joints.HasValue)
            {
                var temp = Filters.FilterJoints(allExercises.AsQueryable(), viewModel.Joints);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.EquipmentIds != null)
            {
                var temp = Filters.FilterEquipmentIds(allExercises.AsQueryable(), viewModel.EquipmentIds);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.OnlyWeights.HasValue)
            {
                var temp = Filters.FilterOnlyWeights(allExercises.AsQueryable(), viewModel.OnlyWeights.Value == Models.NoYes.Yes);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.OnlyAntiGravity.HasValue)
            {
                var temp = Filters.FilterAntiGravity(allExercises.AsQueryable(), viewModel.OnlyAntiGravity.Value == Models.NoYes.Yes);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.ExerciseType.HasValue)
            {
                var temp = Filters.FilterExerciseType(allExercises.AsQueryable(), viewModel.ExerciseType);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.ExerciseSection.HasValue)
            {
                var temp = Filters.FilterExerciseSection(allExercises.AsQueryable(), viewModel.ExerciseSection);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.MovementPatterns.HasValue)
            {
                var temp = Filters.FilterMovementPattern(allExercises.AsQueryable(), viewModel.MovementPatterns);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.MuscleContractions.HasValue)
            {
                var temp = Filters.FilterMuscleContractions(allExercises.AsQueryable(), viewModel.MuscleContractions);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }

            if (viewModel.MuscleMovement.HasValue)
            {
                var temp = Filters.FilterMuscleMovement(allExercises.AsQueryable(), viewModel.MuscleMovement);
                allExercises = allExercises.Where(e => !temp.Contains(e)).ToList();
            }
        }

        if (viewModel.ShowStaticImages)
        {
            // FIXME: Find a better way.
            allExercises.ForEach(e =>
            {
                e.Variation.AnimatedImage = null;
            });
        }

        viewModel.Exercises = allExercises;

        return View(viewModel);
    }
}
