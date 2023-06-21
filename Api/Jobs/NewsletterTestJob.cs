using Api.Controllers;
using Data.Data;
using Quartz;
using System.Net;
using Data.Entities.User;
using Core.Models.Options;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace Api.Jobs;

public class NewsletterTestJob : IJob, IScheduled
{
    private static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    private readonly NewsletterController _newsletterController;
    private readonly UserController _userController;
    private readonly CoreContext _coreContext;
    private readonly HttpClient _httpClient;
    private readonly MailSender _mailSender;
    private readonly IOptions<SiteSettings> _siteSettings;

    public NewsletterTestJob(MailSender mailSender, HttpClient httpClient, IOptions<SiteSettings> siteSettings, NewsletterController newsletterController, UserController userController, CoreContext coreContext)
    {
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
            var users = await _coreContext.Users
                .Where(u => u.Email.EndsWith("@test.aworkoutaday.com"))
                .Where(u => u.DisabledReason == null)
                .ToListAsync();

            foreach (var user in users)
            {
                try
                {
                    var token = new UserToken(user.Id, _userController.CreateToken())
                    {
                        // Token musct be valid for 3 months by law
                        Expires = DateOnly.FromDateTime(DateTime.UtcNow).AddMonths(3).AddDays(3)
                    };
                    user.UserTokens.Add(token);
                    await _coreContext.SaveChangesAsync();

                    var html = await _httpClient.GetAsync($"https://aworkoutaday.com/newsletter/{user.Email}?token={token.Token}");
                    if (html.StatusCode == HttpStatusCode.OK)
                    {
                        var htmlContent = await html.Content.ReadAsStringAsync();
                        await _mailSender.SendMail("newsletter@aworkoutaday.com", user.Email, "Daily Workout", htmlContent);
                        // Don't want to spam the server
                        await Task.Delay(1000);
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

    public static JobKey JobKey => new(nameof(NewsletterTestJob) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(NewsletterTestJob) + "Trigger", GroupName);
    public static string GroupName => "Newsletter";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<NewsletterTestJob>()
            .WithIdentity(JobKey)
            .Build();

        // Trigger the job to run on manual invocation
        var trigger = TriggerBuilder.Create()
            .WithIdentity(TriggerKey)
            .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 0))
            .Build();

        if ((await scheduler.GetTrigger(trigger.Key)) != null)
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
