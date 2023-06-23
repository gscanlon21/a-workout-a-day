using Data.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Cleanup;

public class DeleteOldNewsletters : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;

    public DeleteOldNewsletters(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _coreContext.Newsletters
                .Where(u => u.Date < Today.AddMonths(-1 * Core.User.Consts.DeleteLogsAfterXMonths))
                .ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    public static JobKey JobKey => new(nameof(DeleteOldNewsletters) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DeleteOldNewsletters) + "Trigger", GroupName);
    public static string GroupName => "Newsletter";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<DeleteOldNewsletters>()
            .WithIdentity(JobKey)
            .Build();

        // Trigger the job every day
        var trigger = TriggerBuilder.Create()
            .WithIdentity(TriggerKey)
            .WithDailyTimeIntervalSchedule(x => x.OnEveryDay())
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
