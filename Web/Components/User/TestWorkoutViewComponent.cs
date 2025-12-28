using Core.Models.User;
using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Code.TempData;
using Web.Views.Shared.Components.TestWorkout;

namespace Web.Components.User;

public class TestWorkoutViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public TestWorkoutViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "TestWorkout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.Users.User user, string token)
    {
        // Workouts cannot send until the user has confirmed their account.
        // User has not confirmed their account, let the backfill finish first.
        if (!user.LastActive.HasValue || !user.Features.HasFlag(Features.Admin))
        {
            return Content("");
        }
        else if (user.CreatedDate == DateHelpers.Today)
        {
            // Check to see if the backfill has finished filling the full amount of data. So the muscle targets are accurate.
            var (weeks, _) = await _userRepo.GetWeeklyMuscleVolume(user, UserConsts.TrainingVolumeWeeks, includeToday: true);
            if (weeks < UserConsts.TrainingVolumeWeeks)
            {
                return Content("");
            }
        }

        // Use the persistent token so the user can bookmark this.
        token = await _userRepo.GetPersistentToken(user) ?? token;
        var testSplitOffset = int.TryParse(TempData.Peek(TempData_User.TestSplit)?.ToString(), out int testSplit) ? testSplit : 0;
        var date = DateHelpers.Today.AddMonths(-UserConsts.TestWorkoutsAfterXMonths).AddDays(testSplitOffset);
        var (rotation, frequency) = await _userRepo.GetCurrentWorkoutRotation(user, date);
        return View("TestWorkout", new TestWorkoutViewModel()
        {
            Date = date,
            User = user,
            Token = token,
            Rotation = rotation,
            Frequency = frequency,
        });
    }
}
