﻿using Core.Models.Exercise;
using Data.Entities.User;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Code.TempData;

namespace Web.Controllers.User;

public partial class UserController
{
    /// <summary>
    /// Clears muscle target data over 1 month old.
    /// </summary>
    [HttpPost, Route("muscle/clear")]
    public async Task<IActionResult> ClearMuscleTargetData(string email, string token)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        // Delete all workouts older than today, so the user's current workout doesn't change.
        // Muscle adjustments ignore today, so leaving the current workout won't affect those.
        await _context.UserWorkouts.Where(uw => uw.UserId == user.Id)
            .Where(uw => uw.Date != DateHelpers.Today).ExecuteDeleteAsync();

        // Back-fill several weeks of workout data so muscle targets can take effect immediately.
        await _newsletterService.Backfill(user.Email, token);

        TempData[TempData_User.SuccessMessage] = "Your muscle target data has been reset!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("muscle/reset")]
    public async Task<IActionResult> ResetMuscleRanges(string email, string token, [Bind(Prefix = "muscleGroup")] MusculoskeletalSystem muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        await _context.UserMuscleStrengths
            .Where(um => um.User.Id == user.Id)
            .Where(um => muscleGroups.HasFlag(um.MuscleGroup))
            .ExecuteDeleteAsync();

        TempData[TempData_User.SuccessMessage] = "Your muscle targets have been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("muscle/start/decrease")]
    public async Task<IActionResult> DecreaseStartMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MusculoskeletalSystem muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = Math.Max(UserMuscleStrength.MuscleTargetMin, UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value - UserConsts.IncrementMuscleTargetBy),
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value
                });
            }
            else
            {
                userMuscleGroup.Start = Math.Max(UserMuscleStrength.MuscleTargetMin, userMuscleGroup.Start - UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("muscle/start/increase")]
    public async Task<IActionResult> IncreaseStartMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MusculoskeletalSystem muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value + UserConsts.IncrementMuscleTargetBy,
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value
                });
            }
            else
            {
                userMuscleGroup.Start = Math.Min(userMuscleGroup.End - UserConsts.IncrementMuscleTargetBy, userMuscleGroup.Start + UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("muscle/end/decrease")]
    public async Task<IActionResult> DecreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MusculoskeletalSystem muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value,
                    End = UserMuscleStrength.MuscleTargets[muscleGroup].End.Value - UserConsts.IncrementMuscleTargetBy
                });
            }
            else
            {
                userMuscleGroup.End = Math.Max(userMuscleGroup.Start + UserConsts.IncrementMuscleTargetBy, userMuscleGroup.End - UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }

    [HttpPost, Route("muscle/end/increase")]
    public async Task<IActionResult> IncreaseEndMuscleRange(string email, string token, [Bind(Prefix = "muscleGroup")] MusculoskeletalSystem muscleGroups)
    {
        var user = await _userRepo.GetUser(email, token);
        if (user == null)
        {
            return View("StatusMessage", new StatusMessageViewModel(LinkExpiredMessage));
        }

        var muscleTargetMax = UserMuscleStrength.MuscleTargets.Values.MaxBy(v => v.End.Value).End.Value;
        foreach (var muscleGroup in UserMuscleStrength.MuscleTargets.Keys.Where(mg => muscleGroups.HasFlag(mg)))
        {
            var userMuscleGroup = await _context.UserMuscleStrengths.FirstOrDefaultAsync(um => um.User.Id == user.Id && um.MuscleGroup == muscleGroup);
            if (userMuscleGroup == null)
            {
                _context.UserMuscleStrengths.Add(new UserMuscleStrength()
                {
                    UserId = user.Id,
                    MuscleGroup = muscleGroup,
                    Start = UserMuscleStrength.MuscleTargets[muscleGroup].Start.Value,
                    End = Math.Min(muscleTargetMax, UserMuscleStrength.MuscleTargets[muscleGroup].End.Value + UserConsts.IncrementMuscleTargetBy)
                });
            }
            else
            {
                userMuscleGroup.End = Math.Min(muscleTargetMax, userMuscleGroup.End + UserConsts.IncrementMuscleTargetBy);

                // If the user target matches the default, delete this range so that any default updates take effect.
                if (userMuscleGroup.Range.Equals(UserMuscleStrength.MuscleTargets[muscleGroup]))
                {
                    _context.UserMuscleStrengths.Remove(userMuscleGroup);
                }
            }
        }

        await _context.SaveChangesAsync();
        TempData[TempData_User.SuccessMessage] = "Your muscle target has been updated!";
        return RedirectToAction(nameof(UserController.Edit), new { email, token });
    }
}
