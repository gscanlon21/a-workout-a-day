using Core.Models.User;
using Data;
using Data.Entities.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.UpsertExercise;
using Web.Views.User;

namespace Web.Components.UserExercise;

/// <summary>
/// Note that we're not showing variations that don't fall in the same section.
/// This is so the user is able to manage the variations from this page.
/// </summary>
public class UpsertExerciseViewComponent : ViewComponent
{
    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "UpsertExercise";

    private readonly CoreContext _context;

    public UpsertExerciseViewComponent(CoreContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync(User user, ManageExerciseVariationViewModel.Params parameters)
    {
        if (!user.Features.HasFlag(Features.Debug))
        {
            return Content("");
        }

        var exercise = await _context.Exercises.IgnoreQueryFilters().AsNoTracking()
            .Where(v => v.Id == parameters.ExerciseId)
            .FirstOrDefaultAsync();

        if (exercise == null) { return Content(""); }
        return View("UpsertExercise", new UpsertExerciseViewModel()
        {
            User = user,
            Parameters = parameters,
            Exercise = exercise,
        });
    }
}