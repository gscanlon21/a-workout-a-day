using Core.Models.Options;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Api.Jobs.Update;

/// <summary>
/// Unsubscribes inactive users from the newsletter.
/// </summary>
public class DisableInactiveUsers(ILogger<DisableInactiveUsers> logger, CoreContext coreContext, IOptions<SiteSettings> siteSettings) : IJob, IScheduled
{
    public const string DisabledReason = "No recent activity.";

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var inactiveUsers = await coreContext.Users.IgnoreQueryFilters()
                .Where(u => u.NewsletterDisabledReason == null)
                .Where(u => !u.Email.EndsWith(siteSettings.Value.Domain))
                // User has no account activity in the past X months
                .Where(u => u.LastActive.HasValue && u.LastActive.Value < DateHelpers.Today.AddMonths(-1 * UserConsts.DisableAfterXMonths)
                    || !u.LastActive.HasValue && u.CreatedDate < DateHelpers.Today.AddMonths(-1 * UserConsts.DisableAfterXMonths)
                ).ToListAsync();

            foreach (var user in inactiveUsers)
            {
                user.NewsletterDisabledReason = DisabledReason;
            }

            await coreContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, e, "");
        }
    }

    public static JobKey JobKey => new(nameof(DisableInactiveUsers) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DisableInactiveUsers) + "Trigger", GroupName);
    public static string GroupName => "Update";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<DisableInactiveUsers>()
            .WithIdentity(JobKey)
            .Build();

        // Trigger the job every day
        var trigger = TriggerBuilder.Create()
            .WithIdentity(TriggerKey)
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
            .Build();

        if (await scheduler.GetTrigger(trigger.Key) != null)
        {
            // Update
            await scheduler.RescheduleJob(trigger.Key, trigger);
        }
        else
        {
            // Create
            await scheduler.ScheduleJob(job, trigger);
        }
    }
}
