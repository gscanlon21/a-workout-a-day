using FinerFettle.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinerFettle.Functions.Functions;

public class UserFunctions
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;

    public UserFunctions(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    /// <summary>
    /// Disable inactive users.
    /// </summary>
    [Function(nameof(DisableInactiveUsers))]
    public async Task DisableInactiveUsers([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Core.Debug.Consts.IsDebug)] TimerInfo timerInfo)
    {
        const string disabledReason = "No recent account activity.";

        var inactiveUsers = await _coreContext.Users
            .Where(u => u.DisabledReason == null)
            // User has no account activity in the past X months
            .Where(u => (u.LastActive.HasValue && u.LastActive.Value < Today.AddMonths(-1 * Core.User.Consts.DisableAfterXMonths))
                || (!u.LastActive.HasValue && u.CreatedDate < Today.AddMonths(-1 * Core.User.Consts.DisableAfterXMonths))
            )
            .ToListAsync();

        foreach (var user in inactiveUsers)
        {
            user.DisabledReason = disabledReason;
        }

        await _coreContext.SaveChangesAsync();
    }

    /// <summary>
    /// Delete inactive users.
    /// </summary>
    [Function(nameof(DeleteInactiveUsers))]
    public async Task DeleteInactiveUsers([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Core.Debug.Consts.IsDebug)] TimerInfo timerInfo)
    {
        await _coreContext.Users
            // User is disabled
            .Where(u => u.DisabledReason != null)
            // User has not been active in the past X months
            .Where(u => (u.LastActive != null && u.LastActive < Today.AddMonths(-1 * Core.User.Consts.DeleteAfterXMonths))
                || (u.LastActive == null && u.CreatedDate < Today.AddMonths(-1 * Core.User.Consts.DeleteAfterXMonths))
            ).ExecuteDeleteAsync();
    }

    /// <summary>
    /// Delete expired tokens after 1 day.
    /// </summary>
    [Function(nameof(DeleteOldTokens))]
    public async Task DeleteOldTokens([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Core.Debug.Consts.IsDebug)] TimerInfo timerInfo)
    {
        await _coreContext.UserTokens
            .Where(u => u.Expires < Today.AddDays(-1))
            .ExecuteDeleteAsync();
    }
}
