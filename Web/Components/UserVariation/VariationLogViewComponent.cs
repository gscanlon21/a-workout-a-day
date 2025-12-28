using Core.Models.Newsletter;
using Data;
using Data.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.VariationLog;
using Web.Views.User;

namespace Web.Components.UserVariation;

public class VariationLogViewComponent(CoreContext context) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "VariationLog";

    public async Task<IViewComponentResult> InvokeAsync(User user, ManageExerciseVariationViewModel.Params parameters)
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

        var userWeights = await context.UserVariationLogs
            .Where(uw => uw.UserVariationId == userVariation.Id)
            .ToListAsync();

        return View("VariationLog", new VariationLogViewModel(userWeights, userVariation)
        {
            IsWeighted = userVariation.Variation.IsWeighted
        });
    }
}