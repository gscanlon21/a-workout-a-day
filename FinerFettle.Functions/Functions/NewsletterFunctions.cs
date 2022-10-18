using System;
using System.Linq;
using FinerFettle.Functions.Data;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FinerFettle.Functions.Functions
{
    public class NewsletterFunctions
    {
        private readonly CoreContext _coreContext;

        public NewsletterFunctions(CoreContext coreContext)
        {
            _coreContext = coreContext;
        }

        [FunctionName("DisableNeverActiveUsers")]
        public async Task DisableNeverActiveUsers([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# DisableNeverActiveUsers timer trigger function executed at: {DateTime.Now}");

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

        [FunctionName("DisableInactiveUsers")]
        public async Task DisableInactiveUsers([TimerTrigger(/*Weekly*/ "0 0 0 * * 0", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# DisableInactiveUsers timer trigger function executed at: {DateTime.Now}");

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

        [FunctionName("DeleteInactiveUsers")]
        public async Task DeleteInactiveUsers([TimerTrigger(/*Monthly*/ "0 0 0 1 * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# DeleteInactiveUsers timer trigger function executed at: {DateTime.Now}");

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
}
