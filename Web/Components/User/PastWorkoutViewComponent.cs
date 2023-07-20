using Data.Data;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.ViewModels.User.Components;

namespace Web.Components.User;

public class PastWorkoutViewComponent : ViewComponent
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    /// <summary>
    /// For routing
    /// </summary>
    public const string Name = "PastWorkout";

    private readonly UserRepo _userRepo;
    private readonly CoreContext _context;

    public PastWorkoutViewComponent(CoreContext context, UserRepo userRepo)
    {
        _context = context;
        _userRepo = userRepo;
    }

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user)
    {
        // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
        // Dummy records that are created when the user advances their workout split may also have the same date.
        // If a workout was sent today, we want to skip it. If one wasn't sent yet today, don't skip anything.
        var todaysWorkoutId = (await _context.UserWorkouts
            .Where(uw => uw.UserId == user.Id)
            .Where(uw => uw.Date == Today)
            .OrderByDescending(uw => uw.Id)
            .FirstOrDefaultAsync())?.Id;

        var pastWorkouts = await _context.UserWorkouts
            .Where(uw => uw.UserId == user.Id)
            .Where(uw => uw.Id != todaysWorkoutId)
            .OrderByDescending(uw => uw.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .Take(7)
            .ToListAsync();

        if (!pastWorkouts.Any())
        {
            return Content("");
        }

        return View("PastWorkout", new PastWorkoutViewModel()
        {
            User = user,
            Token = await _userRepo.AddUserToken(user, durationDays: 1),
            PastWorkouts = pastWorkouts,
        });
    }
}
