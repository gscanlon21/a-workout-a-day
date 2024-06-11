using Core.Models.Newsletter;
using Data;
using Data.Dtos.Newsletter;
using Data.Query.Builders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code;
using Web.ViewModels.Components.UserVariation;
using Web.ViewModels.User;

namespace Web.Components.UserVariation;


public class VariationLogViewComponent(CoreContext context, IServiceScopeFactory serviceScopeFactory) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "VariationLog";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, ManageExerciseVariationViewModel.Params parameters)
    {
        var userVariation = await context.UserVariations
            .IgnoreQueryFilters()
            .Include(p => p.Variation)
            // Variations are managed per section, so ignoring variations for .None sections that are only for managing exercises.
            .Where(uv => uv.Section == parameters.Section && parameters.Section != Section.None)
            .FirstOrDefaultAsync(p => p.UserId == user.Id && p.VariationId == parameters.VariationId);

        if (userVariation == null)
        {
            return Content("");
        }

        var userWeights = await context.UserVariationWeights
            .Where(uw => uw.UserVariationId == userVariation.Id)
            .ToListAsync();

        var variations = (await new QueryBuilder(parameters.Section)
            .WithUser(user, ignoreProgressions: true, ignorePrerequisites: true, ignoreIgnored: true, ignoreMissingEquipment: true, uniqueExercises: false)
            .WithExercises(x =>
            {
                x.AddVariations([userVariation.Variation]);
            })
            .Build()
            .Query(serviceScopeFactory))
            .Select(r => new ExerciseVariationDto(r)
            .AsType<Lib.ViewModels.Newsletter.ExerciseVariationViewModel, ExerciseVariationDto>()!)
            .DistinctBy(vm => vm.Variation)
            .ToList();

        return View("VariationLog", new VariationLogViewModel(userWeights, userVariation)
        {
            IsWeighted = userVariation.Variation.IsWeighted
        });
    }
}