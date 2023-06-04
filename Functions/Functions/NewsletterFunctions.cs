using FinerFettle.Functions.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FinerFettle.Functions.Functions;

public class NewsletterFunctions
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;

    public NewsletterFunctions(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    /// <summary>
    /// Delete newsletter logs after X months.
    /// </summary>
    [Function(nameof(DeleteOldNewsletters))]
    public async Task DeleteOldNewsletters([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Core.Debug.Consts.IsDebug)] TimerInfo timerInfo)
    {
        await _coreContext.Newsletters
            .Where(u => u.Date < Today.AddMonths(-1 * Core.User.Consts.DeleteLogsAfterXMonths))
            .ExecuteDeleteAsync();
    }
}
