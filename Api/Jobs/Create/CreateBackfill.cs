using Core.Consts;
using Data.Entities.User;
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

            // Reverse the dates (oldest to newest) so the workout split is calculated properly. Create a workout for every other day.
            var workoutsPerWeek = (await _userRepo.GetWeeklyRotations(user, user.Frequency)).Count; // Divide last so the we round after multiplying.
            var dates = new Stack<DateOnly>(Enumerable.Range(1, UserConsts.TrainingVolumeWeeks * workoutsPerWeek).Select(r => DateHelpers.Today.AddDays(-7 * r / workoutsPerWeek)));

            // Run with max workoutsPerWeek at a time so the training volume weeks is re-calculated with up-to-date data each week.
            var options = new ParallelOptions() { MaxDegreeOfParallelism = workoutsPerWeek, CancellationToken = context.CancellationToken };
            await Parallel.ForEachAsync(dates, options, async (date, cancellationToken) =>
            {
                try
                {
                    // Create a new instance because we're in a parallel loop and CoreContext isn't thread-safe.
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
