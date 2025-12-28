using Core.Dtos.Newsletter;
using Core.Models.Newsletter;
using Data;
using Data.Query;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.ManageVariation;
using Web.Views.User;

namespace Web.Components.UserVariation;

public class ManageVariationViewComponent : ViewComponent
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly CoreContext _context;

    public ManageVariationViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _context = context;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "ManageVariation";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        // UserVariation's are created when querying for a variation.
        var userVariation = await _context.UserVariations
            .IgnoreQueryFilters().AsNoTracking()
            .Include(p => p.Variation)
            // Variations are managed per section, so ignoring variations for .None sections that are only for managing exercises.
            .Where(uv => uv.Section == parameters.Section && parameters.Section != Section.None)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == parameters.VariationId);

        if (userVariation == null) { return Content(""); }
        var exerciseVariation = (await new QueryBuilder(parameters.Section)
            .WithExercises(x =>
            {
                x.AddVariations([userVariation]);
            })
            .Build()
            .Query(_serviceScopeFactory, OrderBy.None))
            .SingleOrDefault();

        if (exerciseVariation == null) { return Content(""); }
        return View("ManageVariation", new ManageVariationViewModel()
        {
            User = user,
            Parameters = parameters,
            UserVariation = userVariation,
            ExerciseVariation = exerciseVariation?.AsType<ExerciseVariationDto>()!,
            LagRefreshXWeeks = userVariation.LagRefreshXWeeks,
            PadRefreshXWeeks = userVariation.PadRefreshXWeeks,
            Weight = userVariation.Weight,
            Notes = userVariation.Notes,
            Sets = userVariation.Sets,
            Reps = userVariation.Reps,
            Secs = userVariation.Secs,
        });
    }
}