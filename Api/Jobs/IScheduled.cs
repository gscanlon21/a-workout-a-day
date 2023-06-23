using Quartz;

namespace Api.Jobs;

public interface IScheduled
{
    public static JobKey JobKey { get; } = null!;
    public static TriggerKey TriggerKey { get; } = null!;
    public static string GroupName { get; } = null!;
    static abstract Task Schedule(IScheduler scheduler);
}
