using Core.Models.Exercise;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.MuscleTargets;

namespace Web.Components.Users;

/// <summary>
/// Renders an alert box summary of how often each muscle the user has worked over the course of a month.
/// </summary>
public class MuscleTargetsViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public MuscleTargetsViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "MuscleTargets";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        if (user == null)
        {
            return Content(string.Empty);
        }

        var weeks = int.TryParse(Request.Query["weeks"], out int weeksTmp) ? weeksTmp : UserConsts.TrainingVolumeWeeks;
        var includeToday = !bool.TryParse(Request.Query["includeToday"], out bool includeTodayTemp) || includeTodayTemp;
        var (weeksOfData, weeklyMuscles) = await _userRepo.GetWeeklyMuscleVolume(user, weeks, includeToday: includeToday);
        if (weeklyMuscles == null)
        {
            return Content(string.Empty);
        }

        var usersWorkedMuscles = (await _userRepo.GetUpcomingRotations(user, user.Frequency)).Aggregate(MusculoskeletalSystem.None, (curr, n) => curr | n.MuscleGroups.Aggregate(MusculoskeletalSystem.None, (curr2, n2) => curr2 | n2));
        return View("MuscleTargets", new MuscleTargetsViewModel()
        {
            User = user,
            Token = token,
            Weeks = weeks,
            WeeksOfData = weeksOfData,
            WeeklyVolume = weeklyMuscles,
            UsersWorkedMuscles = usersWorkedMuscles,
        });
    }
}
