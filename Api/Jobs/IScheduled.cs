using Quartz;

namespace Api.Jobs;

public interface IScheduled
{
    public static JobKey JobKey { get; }
    public static TriggerKey TriggerKey { get; }
    public static string GroupName { get; }
    static abstract Task Schedule(IScheduler scheduler);
}
