using Core.Code.Extensions;
using Core.Models.Exercise;
using Core.Models.User;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Models.Newsletter;
using Data.Repos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

/// <summary>
/// User helpers.
/// 
/// TODO: User 'forgot password' email. Send them a new token in an email so they can regain access to their account.
/// </summary>
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    /// <summary>
    /// Today's date in UTC.
    /// </summary>
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _context;
    private readonly UserRepo _userRepo;

    public UserController(CoreContext context, UserRepo userRepo)
    {
        _userRepo = userRepo;
        _context = context;
    }

    /// <summary>
    /// Grab a user from the db with a specific token
    /// </summary>
    [HttpGet("GetUser")]
    public async Task<User?> GetUser(string email, string token,
        bool includeUserEquipments = false,
        bool includeUserExerciseVariations = false,
        bool includeExerciseVariations = false,
        bool includeMuscles = false,
        bool includeFrequencies = false,
        bool allowDemoUser = false)
    {
        return await _userRepo.GetUser(email, token,
            includeUserEquipments: includeUserEquipments,
            includeUserExerciseVariations: includeUserExerciseVariations,
            includeExerciseVariations: includeExerciseVariations,
            includeMuscles: includeMuscles,
            includeFrequencies: includeFrequencies,
            allowDemoUser: allowDemoUser);
    }

    /// <summary>
    /// Checks if the user should deload for a week.
    /// 
    /// Deloads are weeks with a message to lower the intensity of the workout so muscle growth doesn't stagnate.
    /// Also to ease up the stress on joints.
    /// </summary>
    [HttpGet("CheckNewsletterDeloadStatus")]
    public async Task<(bool needsDeload, TimeSpan timeUntilDeload)> CheckNewsletterDeloadStatus(User user)
    {
        var lastDeload = await _context.Newsletters.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            .OrderByDescending(n => n.Date)
            .FirstOrDefaultAsync(n => n.IsDeloadWeek);

        // Grabs the date of Sunday of the current week.
        var currentWeekStart = Today.AddDays(-1 * (int)Today.DayOfWeek);
        // Grabs the Sunday that was the start of the last deload.
        var lastDeloadStartOfWeek = lastDeload != null ? lastDeload.Date.AddDays(-1 * (int)lastDeload.Date.DayOfWeek) : DateOnly.MinValue;
        // Grabs the Sunday at or before the user's created date.
        var createdDateStartOfWeek = user.CreatedDate.AddDays(-1 * (int)user.CreatedDate.DayOfWeek);
        // How far away the last deload need to be before another deload.
        var countupToNextDeload = Today.AddDays(-7 * user.DeloadAfterEveryXWeeks);

        bool isSameWeekAsLastDeload = lastDeload != null && lastDeloadStartOfWeek == currentWeekStart;
        TimeSpan timeUntilDeload = (isSameWeekAsLastDeload, lastDeload) switch
        {
            // There's never been a deload before, calculate the next deload date using the user's created date.
            (false, null) => TimeSpan.FromDays(createdDateStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Calculate the next deload date using the last deload's date.
            (false, not null) => TimeSpan.FromDays(lastDeloadStartOfWeek.DayNumber - countupToNextDeload.DayNumber),
            // Dates are the same week. Keep the deload going until the week is over.
            _ => TimeSpan.Zero
        };

        return (timeUntilDeload <= TimeSpan.Zero, timeUntilDeload);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation")]
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user)
    {
        return (await GetCurrentAndUpcomingRotations(user)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation2")]
    public async Task<NewsletterRotation> GetTodaysNewsletterRotation(User user, Frequency frequency)
    {
        return (await GetCurrentAndUpcomingRotations(user, frequency)).First();
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetTodaysNewsletterRotation3")]
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user)
    {
        return await GetCurrentAndUpcomingRotations(user, user.Frequency);
    }

    /// <summary>
    /// Calculates the user's next newsletter type (strength/stability/cardio) from the previous newsletter.
    /// </summary>
    [HttpGet("GetCurrentAndUpcomingRotations")]
    public async Task<NewsletterTypeGroups> GetCurrentAndUpcomingRotations(User user, Frequency frequency)
    {
        var previousNewsletter = await _context.Newsletters.AsNoTracking().TagWithCallSite()
            .Where(n => n.UserId == user.Id)
            // Get the previous newsletter from the same rotation group.
            // So that if a user switches frequencies, they continue where they left off.
            .Where(n => n.Frequency == frequency)
            .OrderByDescending(n => n.Date)
            // For testing/demo. When two newsletters get sent in the same day, I want a different exercise set.
            // Dummy records that are created when the user advances their workout split may also have the same date.
            .ThenByDescending(n => n.Id)
            .FirstOrDefaultAsync();

        return new NewsletterTypeGroups(user, frequency, previousNewsletter?.NewsletterRotation);
    }
}
