using Core.Dtos.Newsletter;
using Core.Models.Exercise.Skills;
using Core.Models.Newsletter;
using Data.Query;
using Data.Query.Builders;
using Data.Query.Builders.MuscleGroup;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using Web.Code.Attributes;
using Web.Views.Exercises;
using static Core.Code.Extensions.EnumerableExtensions;
using static System.Net.WebRequestMethods;

namespace Web.Controllers.Exercises;

[Route($"{Name}")]
public class ExercisesController : ViewController
{
    /// <summary>
    /// The name of the controller for routing purposes.
    /// </summary>
    public const string Name = "Exercises";

    /// <summary>
    /// Sections to use if the user has not selected any other filters.
    /// </summary>
    private const Section NullSections = Section.Main | Section.Warmup | Section.Cooldown;

    private readonly IServiceScopeFactory _serviceScopeFactory;

    private static readonly JsonSerializerOptions Options = new()
    {
        // Reduce the size of the serilized string for memory usage.
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
    };

    public ExercisesController(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    [Route(""), AcceptVerbs(Http.Get, Http.Post)]
    [ResponseCompression(Enabled = !DebugConsts.IsDebug)]
    public async Task<IActionResult> All(ExercisesViewModel? viewModel = null)
    {
        viewModel ??= new ExercisesViewModel();
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        // Filtering down to the main/warmup/cooldown sections if nothing is filtered to reduce memory/CPU utilization.
        var queryBuilder = new SystemQueryBuilder(viewModel.Section ?? (viewModel.FormHasData ? Section.None : NullSections))
            .WithSelectionOptions(options =>
            {
                options.IncludePrerequisites = viewModel.FormHasData;
            });

        if (viewModel.Equipment.HasValue)
        {
            queryBuilder = queryBuilder.WithEquipment(viewModel.Equipment.Value);
        }

        if (viewModel.StrengthMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleGroupBuilder
                .WithMuscleGroups([viewModel.StrengthMuscle.Value]), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Strengthens;
            });
        }

        if (viewModel.StretchMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleGroupBuilder
                .WithMuscleGroups([viewModel.StretchMuscle.Value]), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Stretches;
            });
        }

        if (viewModel.SecondaryMuscle.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleGroups(MuscleGroupBuilder
                .WithMuscleGroups([viewModel.SecondaryMuscle.Value]), x =>
            {
                x.MuscleTarget = vm => vm.Variation.Stabilizes;
            });
        }

        if (viewModel.VocalSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(typeof(VocalSkills), viewModel.VocalSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.VisualSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(typeof(VisualSkills), viewModel.VisualSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.CervicalSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(typeof(CervicalSkills), viewModel.CervicalSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.ThoracicSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(typeof(ThoracicSkills), viewModel.ThoracicSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.LumbarSkills.HasValue)
        {
            queryBuilder = queryBuilder.WithSkills(typeof(LumbarSkills), viewModel.LumbarSkills, options =>
            {
                options.RequireSkills = true;
            });
        }

        if (viewModel.ExerciseFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithExerciseFocus([viewModel.ExerciseFocus.Value]);
        }

        if (viewModel.SportsFocus.HasValue)
        {
            queryBuilder = queryBuilder.WithSportsFocus(viewModel.SportsFocus.Value);
        }

        if (viewModel.MovementPatterns.HasValue)
        {
            queryBuilder = queryBuilder.WithMovementPatterns(viewModel.MovementPatterns.Value);
        }

        if (viewModel.MuscleMovement.HasValue)
        {
            queryBuilder = queryBuilder.WithMuscleMovement(viewModel.MuscleMovement.Value);
        }

        var queryResults = await queryBuilder.Build().Query(_serviceScopeFactory, OrderBy.None);
        var filteredResults = FilterExericseVariations(queryResults, Normalize(viewModel.Name));

        // Order exercises that exactly match the searched for name first.
        viewModel.Exercises = filteredResults.OrderByDescending(x => Normalize(x.Exercise.Name)!.Equals(Normalize(viewModel.Name), StringComparison.OrdinalIgnoreCase))
            .ThenBy(x => x.Exercise.Name) // Then order by progression levels.
            .ThenBy(x => x.Variation.Progression.Min, NullOrder.NullsFirst)
            .ThenBy(x => x.Variation.Progression.Max, NullOrder.NullsLast)
            .ThenBy(x => x.Variation.Name)
            .Select(x => x.AsType<ExerciseVariationDto>(Options)!)
            .ToList();

        return View(viewModel);
    }

    /// <summary>
    /// Do this right after the query to reduce json class conversions.
    /// </summary>
    private static IEnumerable<QueryResults> FilterExericseVariations(IEnumerable<QueryResults> queryResults, string? searchText)
    {
        if (!string.IsNullOrWhiteSpace(searchText))
        {
            queryResults = queryResults.Where(x =>
                Normalize(x.Exercise.Name)!.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || Normalize(x.Variation.Name)!.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || Normalize(x.Exercise.Notes)?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true
                || Normalize(x.Variation.Notes)?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true
            );
        }

        return queryResults;
    }

    private static string? Normalize(string? text) => text?.Replace("-", " ")
        .Replace("w/o", "without", StringComparison.OrdinalIgnoreCase)
        .Replace("w/", "with", StringComparison.OrdinalIgnoreCase)
        .Trim();
}
