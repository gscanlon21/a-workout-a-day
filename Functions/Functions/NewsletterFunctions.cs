using FinerFettle.Functions.Data;
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

    [Function(nameof(DeleteOldNewsletters))]
    public async Task DeleteOldNewsletters([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = Core.Debug.Consts.IsDebug)] TimerInfo timerInfo)
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
}
