using Core.Consts;
using Core.Models.Options;
using Core.Models.User;
using Data.Data;
using Data.Entities.Newsletter;
using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using System.Net;

namespace Api.Jobs.Create;

public class NewsletterJob : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly UserRepo _userRepo;
    private readonly NewsletterRepo _newsletterRepo;
    private readonly CoreContext _coreContext;
    private readonly HttpClient _httpClient;
    private readonly IOptions<SiteSettings> _siteSettings;
    private readonly ILogger<NewsletterJob> _logger;

    public NewsletterJob(ILogger<NewsletterJob> logger, UserRepo userRepo, NewsletterRepo newsletterRepo, IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings, CoreContext coreContext)
    {
        _logger = logger;
        _newsletterRepo = newsletterRepo;
        _userRepo = userRepo;
        _coreContext = coreContext;
        _siteSettings = siteSettings;
        _httpClient = httpClientFactory.CreateClient();
        if (_httpClient.BaseAddress != _siteSettings.Value.WebUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.WebUri;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var currentDay = DaysExtensions.FromDate(Today);
            var currentHour = int.Parse(DateTime.UtcNow.ToString("HH"));
            var users = await _coreContext.Users
                // User has confirmed their account.
                .Where(u => u.LastActive.HasValue)
                // User is subscribed to the newsletter.
                .Where(u => u.NewsletterDisabledReason == null)
                // User's send time is now.
                .Where(u => u.SendHour == currentHour)
                // User's send day is now.
                .Where(u => u.SendDays.HasFlag(currentDay) || u.IncludeMobilityWorkouts)
                // User has not received a workout email today.
                .Where(u => !u.UserNewsletters.Where(un => un.Subject == NewsletterConsts.SubjectWorkout).Any(un => un.Date == Today))
                // User is not a test or demo user.
                .Where(u => !u.Email.EndsWith(_siteSettings.Value.Domain) || u.Features.HasFlag(Features.LiveTest) || u.Features.HasFlag(Features.Debug))
                .ToListAsync();

            foreach (var user in users)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var token = await _userRepo.AddUserToken(user, durationDays: 100); // Needs to last at least 3 months by law for unsubscribe link.
                    var html = await _httpClient.GetAsync($"/newsletter/{Uri.EscapeDataString(user.Email)}?token={Uri.EscapeDataString(token)}");
                    if (html.StatusCode == HttpStatusCode.OK)
                    {
                        // Insert newsletter record
                        var userNewsletter = new UserNewsletter(user)
                        {
                            Subject = NewsletterConsts.SubjectWorkout,
                            Body = await html.Content.ReadAsStringAsync(),
                        };

                        _coreContext.UserNewsletters.Add(userNewsletter);
                        await _coreContext.SaveChangesAsync();
                    }
                    else if (html.StatusCode != HttpStatusCode.NoContent)
                    {
                        _logger.Log(LogLevel.Warning, "Newsletter failed for user {Id} with status {StatusCode}", user.Id, html.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e, "Error retrieving newsletter for user {Id}", user.Id);
                }
            }
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error running job {p0}", nameof(NewsletterJob));
        }
    }

    public static JobKey JobKey => new(nameof(NewsletterJob) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(NewsletterJob) + "Trigger", GroupName);
    public static string GroupName => "Create";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<NewsletterJob>()
            .WithIdentity(JobKey)
            .Build();

        // Trigger the job to run every hour on the hour
        var trigger = TriggerBuilder.Create()
            .WithIdentity(TriggerKey)
            // https://www.freeformatter.com/cron-expression-generator-quartz.html
            .WithCronSchedule("0 0,55 * ? * * *")
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
