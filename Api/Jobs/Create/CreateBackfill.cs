using Core.Models.User;
using Data.Entities.Users;
using Data.Repos;
using Quartz;

namespace Api.Jobs.Create;

/// <summary>
/// Creates several workouts for new users, so the muscle targets function better after signing up.
/// </summary>
[DisallowConcurrentExecution]
public class CreateBackfill : IJob, IScheduled
{
    private readonly UserRepo _userRepo;
    private readonly ILogger<CreateBackfill> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CreateBackfill(ILogger<CreateBackfill> logger, IServiceScopeFactory serviceScopeFactory, UserRepo userRepo)
    {
        _logger = logger;
        _userRepo = userRepo;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.Log(LogLevel.Information, "Starting job {p0}", nameof(CreateBackfill));

            var email = context.MergedJobDataMap.GetString("email")!;
            var token = context.MergedJobDataMap.GetString("token")!;
            var user = await _userRepo.GetUserStrict(email, token, includeMuscles: true, includeFrequencies: true);
            var dates = new Stack<DateOnly>(Enumerable.Range(1, UserConsts.TrainingVolumeWeeks * 7)
                .Select(r => user.TodayOffset.AddDays(-r)) // Only keep dates on strengthening days.
                .Where(d => user.SendDays.HasFlag(DaysExtensions.FromDate(d))));

            // For MaxDegreeOfParallelism, don't go over the number of workous in a week
            // ... so the muscle targets are re-calculated with up-to-date data each week.
            // Though let's drop that back to one because we don't show muscle targets as the backfill is progressing anymore.
            // ... And because the parallelism isn't batched, the next week might start before the previous week had finished.
            var options = new ParallelOptions() { MaxDegreeOfParallelism = 1, CancellationToken = context.CancellationToken };
            await Parallel.ForEachAsync(dates, options, async (date, cancellationToken) =>
            {
                try
                {
                    // Create a new scope because we're in a parallel loop and context isn't thread-safe.
                    using var scope = _serviceScopeFactory.CreateScope();
                    var newsletterRepo = scope.ServiceProvider.GetRequiredService<NewsletterRepo>();

                    // Use the same user for all invocations so we're using their the original preferences.
                    // Don't want to use the user's updated preferences while this is going on.
                    await newsletterRepo.Newsletter(user, token, date);
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
