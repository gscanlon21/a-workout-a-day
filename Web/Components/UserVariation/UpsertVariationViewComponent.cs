using Core.Models.User;
using Data;
using Data.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.UpsertVariation;
using Web.Views.User;

namespace Web.Components.UserVariation;

public class UpsertVariationViewComponent : ViewComponent
{
    private readonly CoreContext _context;

    public UpsertVariationViewComponent(CoreContext context)
    {
        _context = context;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "UpsertVariation";

    public async Task<IViewComponentResult> InvokeAsync(User user, ManageExerciseVariationViewModel.Params parameters)
    {
        if (!user.Features.HasFlag(Features.Debug))
        {
            return Content("");
        }

        var variation = await _context.Variations.IgnoreQueryFilters().AsNoTracking()
            .Where(v => v.Id == parameters.VariationId)
            .Include(v => v.Instructions)
            .FirstOrDefaultAsync();

        if (variation == null) { return Content(""); }
        return View("UpsertVariation", new UpsertVariationViewModel()
        {
            User = user,
            Parameters = parameters,
            Variation = variation,
        });
    }
}