using System;
using System.Linq;
using FinerFettle.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;

namespace FinerFettle.Functions.Functions;

public class LogCleanupFunctions
{
    private readonly CoreContext _coreContext;

    public LogCleanupFunctions(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    [Function("DeleteOldNewsletters")]
    public async Task DeleteOldNewsletters([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newslettersToRemove = await _coreContext.Newsletters
            // Delete newsletter logs after 1 year
            .Where(u => u.Date < today.AddYears(-1))
            .ToListAsync();

        foreach (var newsletter in newslettersToRemove)
        {
            _coreContext.Remove(newsletter);
        }

        await _coreContext.SaveChangesAsync();
    }

    [Function("DeleteOldTokens")]
    public async Task DeleteOldTokens([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var userTokensToRemove = await _coreContext.UserTokens
            // Delete expired tokens after 1 month
            .Where(u => u.Expires < today.AddMonths(-1))
            .ToListAsync();

        foreach (var userToken in userTokensToRemove)
        {
            _coreContext.Remove(userToken);
        }

        await _coreContext.SaveChangesAsync();
    }
}
