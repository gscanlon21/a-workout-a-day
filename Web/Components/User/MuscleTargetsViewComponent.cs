using Core.Consts;
using Core.Models.Exercise;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of how often each muscle the user has worked over the course of a month.
/// </summary>
public class MuscleTargetsViewComponent : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "MuscleTargets";

    private readonly UserRepo _userRepo;

    public MuscleTargetsViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        // Add 1 because deloads occur after every x weeks, not on.
        int weeks = int.TryParse(Request.Query["weeks"], out int weeksTmp) ? weeksTmp : Math.Max(UserConsts.DeloadAfterEveryXWeeksDefault, user.DeloadAfterEveryXWeeks + 1);
        var (_, weeklyMuscles) = await _userRepo.GetWeeklyMuscleVolume(user, weeks: weeks);
        var usersWorkedMuscles = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups);

        if (weeklyMuscles == null)
        {
            return Content(string.Empty);
        }

        return View("MuscleTargets", new MuscleTargetsViewModel()
        {
            User = user,
            Weeks = weeks,
            UsersWorkedMuscles = usersWorkedMuscles,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
            WeeklyVolume = weeklyMuscles,
        });
    }
}
