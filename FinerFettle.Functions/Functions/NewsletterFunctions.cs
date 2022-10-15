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

        [FunctionName("DisableInactiveUsers")]
        public async Task Run([TimerTrigger(/*Daily*/ "0 0 0 * * *", RunOnStartup = true)] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# DisableInactiveUsers timer trigger function executed at: {DateTime.Now}");

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var neverActive = await _coreContext.Users
                .Where(u => u.DisabledReason == null)
                // User has received an email
                .Where(u => u.Newsletters.Count() > 0)
                // User never confirmed their account
                .Where(u => u.LastActive == null)
                // Give the user 10 days to confirm their account
                .Where(u => u.CreatedDate < today.AddDays(-10))
                .ToListAsync();

            foreach (var user in neverActive)
            {
                user.DisabledReason = "User did not confirm their account";
            }

            await _coreContext.SaveChangesAsync();


            var inactiveUsers = await _coreContext.Users
                .Where(u => u.DisabledReason == null)
                // User has no account activity in the past year
                .Where(u => u.LastActive != null && u.LastActive <= today.AddYears(-1))
                .ToListAsync();

            foreach (var user in inactiveUsers)
            {
                user.DisabledReason = "No activity in the past year";
            }

            await _coreContext.SaveChangesAsync();
        }
    }
}
