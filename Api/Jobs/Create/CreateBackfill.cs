using Core.Consts;
using Core.Models.Options;
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
public class CreateBackfill : IJob, IScheduled
{
    private readonly UserRepo _userRepo;
    private readonly HttpClient _httpClient;
    private readonly CoreContext _coreContext;
    private readonly ILogger<CreateBackfill> _logger;
    private readonly IOptions<SiteSettings> _siteSettings;

    public CreateBackfill(ILogger<CreateBackfill> logger, UserRepo userRepo, IHttpClientFactory httpClientFactory, IOptions<SiteSettings> siteSettings, CoreContext coreContext)
    {
        _logger = logger;
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
            _logger.Log(LogLevel.Information, "Starting job {p0}", nameof(CreateBackfill));

            var email = context.MergedJobDataMap.GetString("email")!;
            var token = context.MergedJobDataMap.GetString("token")!;
            var user = await _userRepo.GetUserStrict(email, token);

            // Delete old workout data, start fresh.
            await _coreContext.UserWorkouts.IgnoreQueryFilters().Where(uw => uw.UserId == user.Id).ExecuteDeleteAsync();

            // Reverse the dates (oldest to newest) so the workout split is calculated properly. Create a workout for every other day.
            var workoutsPerWeek = (await _userRepo.GetWeeklyRotations(user, user.Frequency)).Count(); // Divide last so the we round after multiplying.
            var dates = new Stack<DateOnly>(Enumerable.Range(1, UserConsts.TrainingVolumeWeeks * workoutsPerWeek).Select(r => DateHelpers.Today.AddDays(-7 * r / workoutsPerWeek)));

            // Try to complete this before the user alters their preferences and messes with the expected training volume.
            var options = new ParallelOptions() { MaxDegreeOfParallelism = 3, CancellationToken = context.CancellationToken };
            await Parallel.ForEachAsync(dates, options, async (date, cancellationToken) =>
            {
                try
                {
                    // Don't hit the user repo because we're in a parallel loop and CoreContext isn't thread-safe.
                    await _httpClient.GetAsync($"/newsletter/{Uri.EscapeDataString(user.Email)}/{date:O}?token={Uri.EscapeDataString(token)}&client={Client.Email}", cancellationToken);
                }
                catch (Exception e)
                {
                    _logger.Log(LogLevel.Error, e, "Error retrieving newsletter for user {Id}", user.Id);
                }
            });
        }
        catch (Exception e)
        {
            _logger.Log(LogLevel.Error, e, "Error running job {p0}", nameof(CreateBackfill));
        }
        finally
        {
            _logger.Log(LogLevel.Information, "Ending job {p0}", nameof(CreateBackfill));
        }
    }

    public static JobKey JobKey => new(nameof(CreateBackfill) + "Job", GroupName);
    public static TriggerKey TriggerKey => new(nameof(CreateBackfill) + "Trigger", GroupName);
    public static string GroupName => "Create";

    public static async Task Schedule(IScheduler scheduler)
    {
        var job = JobBuilder.Create<CreateBackfill>().WithIdentity(JobKey).StoreDurably(true).Build();
        var trigger = TriggerBuilder.Create().WithIdentity(TriggerKey).StartNow().Build();
        await scheduler.AddJob(job, replace: true);
    }

    public static async Task Trigger(IScheduler scheduler, User user, string token)
    {
        await scheduler.TriggerJob(JobKey, new JobDataMap(new Dictionary<string, string>()
        {
            ["email"] = user.Email,
            ["token"] = token
        }));
    }
}
