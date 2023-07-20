using Core.Models.User;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class WorkoutViewComponent : ViewComponent
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "Workout";

    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;

    public WorkoutViewComponent(CoreContext context, UserRepo userRepo)
    {
        _context = context;
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        // User has not confirmed their account, they cannot see a workout yet.
        if (!user.LastActive.HasValue)
        {
            return Content("");
        }

        var (frequency, rotation) = await TodaysRotation(user);
        return View("Workout", new WorkoutViewModel()
        {
            User = user,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
            NextRotation = rotation,
            NextFrequency = frequency
        });
    }

    private async Task<(Frequency frequency, WorkoutRotation? rotation)> TodaysRotation(Data.Entities.User.User user)
    {
        if (user.SendDays.HasFlag(DaysExtensions.FromDate(Today)))
        {
            return (user.Frequency, await _userRepo.GetTodaysWorkoutRotation(user, todays: true));
        }
        else if (user.IncludeMobilityWorkouts)
        {
            return (Frequency.OffDayStretches, await _userRepo.GetTodaysWorkoutRotation(user, Frequency.OffDayStretches, todays: true));
        }

        return (user.Frequency, null);
    }
}
