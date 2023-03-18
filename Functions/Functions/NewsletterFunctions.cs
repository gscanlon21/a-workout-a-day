using FinerFettle.Functions.Data;
using Functions.Code;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinerFettle.Functions.Functions;

public class NewsletterFunctions
{
    private readonly CoreContext _coreContext;

    public NewsletterFunctions(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    [Function("DisableNeverActiveUsers")]
    public async Task DisableNeverActiveUsers([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Debug.RunOnStartup)] TimerInfo myTimer)
    {
        const string disabledReason = "No account activity";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var neverActive = await _coreContext.Users
            .Where(u => u.DisabledReason == null)
            // User has received an email
            .Where(u => u.Newsletters.Count() > 0)
            // User never confirmed their account
            .Where(u => u.LastActive == null)
            // Give the user 3 months to confirm their account
            .Where(u => u.CreatedDate < today.AddMonths(-3))
            .ToListAsync();

        foreach (var user in neverActive)
        {
            user.DisabledReason = disabledReason;
        }

        await _coreContext.SaveChangesAsync();
    }

    [Function("DisableInactiveUsers")]
    public async Task DisableInactiveUsers([TimerTrigger(/*Weekly*/ "0 0 0 * * 0", RunOnStartup = Debug.RunOnStartup)] TimerInfo myTimer)
    {
        const string disabledReason = "No recent account activity";

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inactiveUsers = await _coreContext.Users
            .Where(u => u.DisabledReason == null)
            // User has no account activity in the past 6 months
            .Where(u => (u.LastActive != null && u.LastActive < today.AddMonths(-6))
                || (u.LastActive == null && u.CreatedDate < today.AddMonths(-6))
            )
            .ToListAsync();

        foreach (var user in inactiveUsers)
        {
            user.DisabledReason = disabledReason;
        }

        await _coreContext.SaveChangesAsync();
    }

    [Function("DeleteInactiveUsers")]
    public async Task DeleteInactiveUsers([TimerTrigger(/*Monthly*/ "0 0 0 1 * *", RunOnStartup = Debug.RunOnStartup)] TimerInfo myTimer)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var toDeleteUsers = await _coreContext.Users
            // User is disabled
            .Where(u => u.DisabledReason != null)
            // User has not been active in the past year
            .Where(u => (u.LastActive != null && u.LastActive < today.AddYears(-1))
                || (u.LastActive == null && u.CreatedDate < today.AddYears(-1))
            )
            .ToListAsync();

        foreach (var user in toDeleteUsers)
        {
            _coreContext.Users.Remove(user);
        }

        await _coreContext.SaveChangesAsync();
    }
}
