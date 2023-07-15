using Core.Consts;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Update;

public class DisableInactiveUsers : IJob, IScheduled
{
    public const string DisabledReason = "No recent activity.";

    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;
    private readonly ILogger<DisableInactiveUsers> _logger;

    public DisableInactiveUsers(ILogger<DisableInactiveUsers> logger, CoreContext coreContext)
    {
        _logger = logger;
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var inactiveUsers = await _coreContext.Users.IgnoreQueryFilters()
                .Where(u => u.NewsletterDisabledReason == null)
                // User has no account activity in the past X months
                .Where(u => u.LastActive.HasValue && u.LastActive.Value < Today.AddMonths(-1 * UserConsts.DisableAfterXMonths)
                    || !u.LastActive.HasValue && u.CreatedDate < Today.AddMonths(-1 * UserConsts.DisableAfterXMonths)
                ).ToListAsync();

            foreach (var user in inactiveUsers)
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
