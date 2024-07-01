using Core.Models.User;
using Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Views.Shared.Components.NextWorkout;

namespace Web.Components.User;


/// <summary>
/// Renders an alert box summary of when the user's next workout will become available.
/// </summary>
public class NextWorkoutViewComponent(CoreContext context, UserRepo userRepo) : ViewComponent
{
    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "NextWorkout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        DateOnly? nextSendDate = null;
        if (user.RestDays < Days.All || user.IncludeMobilityWorkouts)
        {
            nextSendDate = DateTime.UtcNow.Hour <= user.SendHour ? DateOnly.FromDateTime(DateTime.UtcNow) : DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
            // Next send date is a rest day and user does not want off day workouts, next send date is the day after.
            while ((user.RestDays.HasFlag(DaysExtensions.FromDate(nextSendDate.Value)) && !user.IncludeMobilityWorkouts)
                // User was sent a workout for the next send date, next send date is the day after.
                || await context.UserWorkouts
                    .Where(n => n.UserId == user.Id)
                    .Where(n => n.Date == nextSendDate.Value)
                    // Checking the newsletter variations because we create a dummy newsletter to advance the workout split.
                    .AnyAsync(n => n.UserWorkoutVariations.Any())
                )
            {
                nextSendDate = nextSendDate.Value.AddDays(1);
            }
        }

        var nextSendDateTime = nextSendDate?.ToDateTime(TimeOnly.FromTimeSpan(TimeSpan.FromHours(user.SendHour)));
        var timeUntilNextSend = !nextSendDateTime.HasValue ? null : nextSendDateTime - DateTime.UtcNow;
        if (!timeUntilNextSend.HasValue)
        {
            return Content("");
        }

        return View("NextWorkout", new NextWorkoutViewModel()
        {
            User = user,
            Token = await userRepo.AddUserToken(user, durationDays: 1),
            CurrentAndUpcomingRotations = await userRepo.GetUpcomingRotations(user, user.Frequency),
            MobilityRotation = (await userRepo.GetUpcomingRotations(user, Frequency.OffDayStretches)).First(),
            TimeUntilNextSend = timeUntilNextSend,
            Today = DaysExtensions.FromDate(user.TodayOffset),
            NextWorkoutSendsToday = timeUntilNextSend.HasValue && DateOnly.FromDateTime(DateTime.UtcNow.Add(timeUntilNextSend.Value)) == user.TodayOffset
        });
    }
}
