﻿using Core.Consts;
using Data.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Cleanup;

public class DisableInactiveUsers : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;

    public DisableInactiveUsers(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _coreContext.Newsletters
                .Where(u => u.Date < Today.AddMonths(-1 * UserConsts.DeleteLogsAfterXMonths))
                .ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    public static JobKey JobKey => new(nameof(DisableInactiveUsers) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DisableInactiveUsers) + "Trigger", GroupName);
    public static string GroupName => "User";

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
