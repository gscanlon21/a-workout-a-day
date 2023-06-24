using Api.Code;
using Api.Controllers;
using Core.Models.Options;
using Data.Data;
using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using System.Net;

namespace Api.Jobs.Newsletter;

public class NewsletterDebugJob : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly NewsletterController _newsletterController;
    private readonly UserController _userController;
    private readonly UserRepo _userRepo;
    private readonly CoreContext _coreContext;
    private readonly HttpClient _httpClient;
    private readonly MailSender _mailSender;
    private readonly IOptions<SiteSettings> _siteSettings;

    public NewsletterDebugJob(UserRepo userRepo, MailSender mailSender, HttpClient httpClient, IOptions<SiteSettings> siteSettings, NewsletterController newsletterController, UserController userController, CoreContext coreContext)
    {
        _userRepo = userRepo;
        _userController = userController;
        _newsletterController = newsletterController;
        _coreContext = coreContext;
        _siteSettings = siteSettings;
        _mailSender = mailSender;
        _httpClient = httpClient;
        if (_httpClient.BaseAddress != _siteSettings.Value.WebUri)
        {
            _httpClient.BaseAddress = _siteSettings.Value.WebUri;
        }
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var user = await _coreContext.Users
                .Where(u => u.Email == "debug@aworkoutaday.com")
                .Where(u => u.DisabledReason == null)
                .FirstOrDefaultAsync();

            if (user != null)
            {
                var token = await _userRepo.AddUserToken(user, durationDays: 100);

                var html = await _httpClient.GetAsync($"https://aworkoutaday.com/newsletter/{user.Email}?token={token}");
                if (html.StatusCode == HttpStatusCode.OK)
                {
                    var htmlContent = await html.Content.ReadAsStringAsync();
                    await _mailSender.SendMail("newsletter@aworkoutaday.com", user.Email, "Daily Workout", htmlContent);
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    public static JobKey JobKey => new(nameof(NewsletterDebugJob) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(NewsletterDebugJob) + "Trigger", GroupName);
    public static string GroupName => "Newsletter";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<NewsletterDebugJob>()
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
