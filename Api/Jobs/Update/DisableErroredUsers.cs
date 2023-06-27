using Data.Data;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Api.Jobs.Update;

public class DisableErroredUsers : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;

    public const string DisabledReason = "Emails are bouncing.";

    public DisableErroredUsers(CoreContext coreContext)
    {
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var erroredUsers = await _coreContext.Users
                .Where(u => u.UserNewsletters
                    .Where(un => un.Date > Today.AddMonths(-1))
                    .Count(un => un.Error != null) > 3)
                .ToListAsync();

            foreach (var user in erroredUsers)
            {
                user.DisabledReason = DisabledReason;
            }

            await _coreContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
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
