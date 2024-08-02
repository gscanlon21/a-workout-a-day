using Core.Consts;
using Core.Models.Options;
using Core.Models.User;
using Data;
using Data.Entities.Newsletter;
using Data.Entities.User;
using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using System.Net;

namespace Api.Jobs.Create;

[DisallowConcurrentExecution]
public class CreateEmails : IJob, IScheduled
{
    private readonly UserRepo _userRepo;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CreateEmails> _logger;
    private readonly IOptions<SiteSettings> _siteSettings;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateEmails(ILogger<CreateEmails> logger, UserRepo userRepo, IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _userRepo = userRepo;
        _siteSettings = siteSettings;
        _serviceScopeFactory = serviceScopeFactory;
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
            _logger.Log(LogLevel.Information, "Starting job {p0}", nameof(CreateEmails));
            var options = new ParallelOptions() { MaxDegreeOfParallelism = 3, CancellationToken = context.CancellationToken };
            await Parallel.ForEachAsync(await GetUsers().ToListAsync(), options, async (userToken, cancellationToken) =>
            {
                try
                {
                    // The creation of DbContext is lightweight, and the context is not thread-safe.
                    using var scope = _serviceScopeFactory.CreateScope();
                    using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

                    var html = await _httpClient.GetAsync($"/newsletter/{Uri.EscapeDataString(userToken.User.Email)}?token={Uri.EscapeDataString(userToken.Token)}", cancellationToken);
                    if (html.StatusCode == HttpStatusCode.OK)
                    {
                        // Insert newsletter record.
                        context.UserEmails.Add(new UserEmail(userToken.User)
                        {
                            Subject = EmailConsts.SubjectWorkout,
                            Body = await html.Content.ReadAsStringAsync(cancellationToken),
                        });

                        await context.SaveChangesAsync(cancellationToken);
                    }
                    else if (html.StatusCode != HttpStatusCode.NoContent)
                    {
                        _logger.Log(LogLevel.Warning, "Newsletter failed for user {Id} with status {StatusCode}", userToken.User.Id, html.StatusCode);
                    }
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e, "Error retrieving newsletter for user {Id}", userToken.User.Id);
                }
            });
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error running job {p0}", nameof(CreateEmails));
        }
        finally
        {
            _logger.Log(LogLevel.Information, "Ending job {p0}", nameof(CreateEmails));
        }
    }

    internal async IAsyncEnumerable<(User User, string Token)> GetUsers()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var currentDay = DaysExtensions.FromDate(DateHelpers.Today);
        var currentHour = int.Parse(DateTime.UtcNow.ToString("HH"));
        foreach (var user in await context.Users.AsNoTracking()
            // User has confirmed their account.
            .Where(u => u.LastActive.HasValue)
            // User is subscribed to the newsletter.
            .Where(u => u.NewsletterDisabledReason == null)
            // User's send time is now.
            .Where(u => u.SendHour == currentHour)
            // User's send day is now.
            .Where(u => u.SendDays.HasFlag(currentDay) || u.IncludeMobilityWorkouts)
            // User has not received a workout email today.
            .Where(u => !u.UserEmails.Where(un => un.Subject == EmailConsts.SubjectWorkout).Any(un => un.Date == DateHelpers.Today))
            // User is not a test or demo user.
            .Where(u => !u.Email.EndsWith(_siteSettings.Value.Domain) || u.Features.HasFlag(Features.Test) || u.Features.HasFlag(Features.Debug))
            .ToListAsync())
        {
            // Token needs to last at least 3 months by law for unsubscribe link.
            yield return (user, await _userRepo.AddUserToken(user, durationDays: 100));
        }
    }

    public static JobKey JobKey => new(nameof(CreateEmails) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(CreateEmails) + "Trigger", GroupName);
    public static string GroupName => "Create";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<CreateEmails>()
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
