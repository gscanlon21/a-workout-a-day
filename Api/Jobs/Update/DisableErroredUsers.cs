using Core.Models.Newsletter;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Update;

/// <summary>
/// Unsubscribes users from the newsletter if sending their emails fails too many times.
/// </summary>
public class DisableErroredUsers : IJob, IScheduled
{
    public const string DisabledReason = "Emails are bouncing.";

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;
    private readonly ILogger<DisableErroredUsers> _logger;

    public DisableErroredUsers(ILogger<DisableErroredUsers> logger, CoreContext coreContext)
    {
        _logger = logger;
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var erroredUsers = await _coreContext.Users.IgnoreQueryFilters()
                .Where(u => u.NewsletterDisabledReason == null)
                .Where(u => u.UserEmails
                    .Where(un => un.Date >= Today.AddMonths(-1))
                    .Count(un => un.EmailStatus == EmailStatus.Failed) > 3)
                .ToListAsync();

            foreach (var user in erroredUsers)
            {
                user.NewsletterDisabledReason = DisabledReason;
            }

            await _coreContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "");
        }
    }

    public static JobKey JobKey => new(nameof(DisableErroredUsers) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DisableErroredUsers) + "Trigger", GroupName);
    public static string GroupName => "Update";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<DisableErroredUsers>()
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
