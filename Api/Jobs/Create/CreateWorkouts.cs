using Core.Code.Helpers;
using Core.Models.Options;
using Core.Models.User;
using Data;
using Data.Entities.User;
using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;

namespace Api.Jobs.Create;

/// <summary>
/// Creates a new workout for app users, so the muscle targets are up-to-date if the user doesn't check the app every day.
/// </summary>
[DisallowConcurrentExecution]
public class CreateWorkouts : IJob, IScheduled
{
    private readonly UserRepo _userRepo;
    private readonly NewsletterRepo _newsletterRepo;
    private readonly CoreContext _coreContext;
    private readonly IOptions<SiteSettings> _siteSettings;
    private readonly ILogger<CreateWorkouts> _logger;

    public CreateWorkouts(ILogger<CreateWorkouts> logger, UserRepo userRepo, NewsletterRepo newsletterRepo, IOptions<SiteSettings> siteSettings, CoreContext coreContext)
    {
        _logger = logger;
        _newsletterRepo = newsletterRepo;
        _userRepo = userRepo;
        _coreContext = coreContext;
        _siteSettings = siteSettings;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            foreach (var user in await GetUsers())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    // Token needs to last at least 3 months by law for unsubscribe link.
                    var token = await _userRepo.AddUserToken(user, durationDays: 100);
                    _ = await _newsletterRepo.Newsletter(user.Email, token);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e, "Error retrieving newsletter for user {Id}", user.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error running job {p0}", nameof(CreateWorkouts));
        }
    }

    internal async Task<List<User>> GetUsers()
    {
        var currentDay = DaysExtensions.FromDate(DateHelpers.Today);
        var currentHour = int.Parse(DateTime.UtcNow.ToString("HH"));
        return await _coreContext.Users
            // User has confirmed their account.
            .Where(u => u.LastActive.HasValue)
            // User is not subscribed to the newsletter.
            .Where(u => u.NewsletterDisabledReason != null)
            // User's send time is now.
            .Where(u => u.SendHour == currentHour)
            // User's send day is now.
            .Where(u => u.SendDays.HasFlag(currentDay) || u.IncludeMobilityWorkouts)
            // User is not a test or demo user.
            .Where(u => !u.Email.EndsWith(_siteSettings.Value.Domain) || u.Features.HasFlag(Features.Test) || u.Features.HasFlag(Features.Debug))
            .ToListAsync();
    }

    public static JobKey JobKey => new(nameof(CreateWorkouts) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(CreateWorkouts) + "Trigger", GroupName);
    public static string GroupName => "Create";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<CreateWorkouts>()
            .WithIdentity(JobKey)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(TriggerKey)
            // https://www.freeformatter.com/cron-expression-generator-quartz.html
            .WithCronSchedule("0 0,30,45,55,59 * ? * * *")
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
