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

    public NewsletterJob(UserRepo userRepo, NewsletterRepo newsletterRepo, IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings, CoreContext coreContext)
    {
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
            // u.Features.HasFlag(Core.Models.User.Features.ManyEmails)
            var currentDay = DaysExtensions.FromDate(Today);
            var currentHour = int.Parse(DateTime.UtcNow.ToString("HH"));
            var users = await _coreContext.Users
                .Where(u => u.SendEmailWorkouts)
                .Where(u => u.DisabledReason == null)
                .Where(u => u.SendHour == currentHour)
                .Where(u => u.SendDays.HasFlag(currentDay) || u.IncludeMobilityWorkouts)
                .Where(u => !u.UserNewsletters.Any(un => un.Date == Today))
                .Where(u => !u.Email.EndsWith("aworkoutaday.com") || u.Features.HasFlag(Features.Test) || u.Features.HasFlag(Features.Debug))
                .ToListAsync();

            foreach (var user in users)
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    var token = await _userRepo.AddUserToken(user, durationDays: 100);
                    var html = await _httpClient.GetAsync($"https://aworkoutaday.com/newsletter/{user.Email}?token={token}");
                    if (html.StatusCode == HttpStatusCode.OK)
                    {
                        // Insert newsletter record
                        var userNewsletter = new UserNewsletter(user)
                        {
                            Subject = "Daily Workout",
                            Body = await html.Content.ReadAsStringAsync(),
                        };

                        _coreContext.UserNewsletters.Add(userNewsletter);
                        await _coreContext.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine(e);
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
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
