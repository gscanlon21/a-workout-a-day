using Core.Consts;
using Core.Models.Options;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Api.Jobs.Delete;

/// <summary>
/// Deletes inactive accounts.
/// </summary>
public class DeleteInactiveUsers : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly CoreContext _coreContext;
    private readonly ILogger<DeleteInactiveUsers> _logger;
    private readonly IOptions<SiteSettings> _siteSettings;

    public DeleteInactiveUsers(ILogger<DeleteInactiveUsers> logger, CoreContext coreContext, IOptions<SiteSettings> siteSettings)
    {
        _siteSettings = siteSettings;
        _logger = logger;
        _coreContext = coreContext;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            await _coreContext.Users.IgnoreQueryFilters()
                .Where(u => !u.Email.EndsWith(_siteSettings.Value.Domain))
                // User has not been active in the past X months
                .Where(u => (u.LastActive != null && u.LastActive < Today.AddMonths(-1 * UserConsts.DeleteAfterXMonths))
                    || (u.LastActive == null && u.CreatedDate < Today.AddMonths(-1 * UserConsts.DeleteAfterXMonths))
                ).ExecuteDeleteAsync();
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "");
        }
    }

    public static JobKey JobKey => new(nameof(DeleteInactiveUsers) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(DeleteInactiveUsers) + "Trigger", GroupName);
    public static string GroupName => "Delete";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<DeleteInactiveUsers>()
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
