using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using Web.Code.Extensions;
using Web.Data;
using Web.Models.Exercise;
using Web.Services;
using Web.ViewModels.User;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of how often each muscle the user has worked over the course of a month.
/// </summary>
public class MonthlyMusclesViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "MonthlyMuscles";

    public const int CalculateOverXWeeks = Entities.User.User.RefreshFunctionalEveryXWeeksMax;

    private readonly CoreContext _context;

    private readonly UserService _userService;

    public MonthlyMusclesViewComponent(CoreContext context, UserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<IViewComponentResult> InvokeAsync(Entities.User.User user)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        int weeks = int.TryParse(Request.Query["weeks"], out int weeksTmp) ? weeksTmp : CalculateOverXWeeks;
        var weeklyMuscles = await _userService.GetWeeklyMuscleVolume(user, avgOverXWeeks: weeks, includeNewToFitness: true);

        if (weeklyMuscles == null)
        {
            return Content(string.Empty);
        }

        return View("MonthlyMuscles", new MonthlyMusclesViewModel()
        {
            User = user,
            WeeklyTimeUnderTension = weeklyMuscles,
            WeeklyTimeUnderTensionAvg = weeklyMuscles.Sum(g => g.Value) / (double)EnumExtensions.GetSingleValues32<MuscleGroups>().Length
        });
    }
}
