﻿using Data.Repos;
using Microsoft.AspNetCore.Mvc;
using Web.Views.Shared.Components.Workout;

namespace Web.Components.User;

public class WorkoutViewComponent : ViewComponent
{
    private readonly UserRepo _userRepo;

    public WorkoutViewComponent(UserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    /// <summary>
    /// For routing.
    /// </summary>
    public const string Name = "Workout";

    public async Task<IViewComponentResult> InvokeAsync(Data.Entities.User.User user, string token)
    {
        // User has not confirmed their account, let the backfill finish first.
        if (!user.LastActive.HasValue)
        {
            return Content("");
        }

        // Use the persistent token so the user can bookmark this.
        token = await _userRepo.GetPersistentToken(user) ?? token;
        var currentWorkout = await _userRepo.GetCurrentWorkoutRotation(user);
        return View("Workout", new WorkoutViewModel()
        {
            User = user,
            Token = token,
            Rotation = currentWorkout.Item1,
            Frequency = currentWorkout.Item2,
        });
    }
}
