using Core.Consts;
using Core.Models.Exercise;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

/// <summary>
/// Renders an alert box summary of how often each muscle the user has worked over the course of a month.
/// </summary>
public class MuscleTargetsViewComponent(UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "MuscleTargets";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        // Add 1 because deloads occur after every x weeks, not on.
        int weeks = int.TryParse(Request.Query["weeks"], out int weeksTmp) ? weeksTmp : user.DeloadAfterEveryXWeeks + 1;
        var (weeksOfData, weeklyMuscles) = await userRepo.GetWeeklyMuscleVolume(user, weeks: weeks);
        var usersWorkedMuscles = (await userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MuscleGroups.None, (curr, n) => curr | n.MuscleGroups.Aggregate(MuscleGroups.None, (curr2, n2) => curr2 | n2));

        if (weeklyMuscles == null)
        {
            return Content(string.Empty);
        }

        return View("MuscleTargets", new MuscleTargetsViewModel()
        {
            User = user,
            Weeks = weeks,
            WeeksOfData = weeksOfData,
            WeeklyVolume = weeklyMuscles,
            UsersWorkedMuscles = usersWorkedMuscles,
            Token = await userRepo.AddUserToken(user, durationDays: 1),
        });
    }
}
