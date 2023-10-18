using Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Delete;

public class DeleteOldTokens(ILogger<DeleteOldTokens> logger, CoreContext coreContext) : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await coreContext.UserTokens.IgnoreQueryFilters()
                .Where(u => u.Expires <= DateTime.UtcNow)
                .ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            logger.Log(LogLevel.Error, e, "");
        }
    }

    public static JobKey JobKey => new(nameof(DeleteOldTokens) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DeleteOldTokens) + "Trigger", GroupName);
    public static string GroupName => "Delete";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<DeleteOldTokens>()
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
