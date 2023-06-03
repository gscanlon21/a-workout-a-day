using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        int weeks = int.TryParse(Request.Query["weeks"], out int weeksTmp) ? weeksTmp : Math.Max(Entities.User.User.RefreshFunctionalEveryXWeeksDefault, user.RefreshFunctionalEveryXWeeks);
        var weeklyMuscles = await _userService.GetWeeklyMuscleVolume(user, avgOverXWeeks: weeks, includeNewToFitness: true);
        var usersWorkedMuscles = (await _userService.GetCurrentAndUpcomingRotations(user)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups);

        if (weeklyMuscles == null)
        {
            return Content(string.Empty);
        }

        return View("MonthlyMuscles", new MonthlyMusclesViewModel()
        {
            User = user,
            Weeks = weeks,
            UsersWorkedMuscles = usersWorkedMuscles,
            Token = await _userService.AddUserToken(user, durationDays: 2),
            WeeklyVolume = weeklyMuscles,
        });
    }
}
