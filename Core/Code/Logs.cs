using Core.Interfaces.User;
using System.Collections.Concurrent;

namespace Core.Code;

public static class Logs
{
    private static readonly ConcurrentDictionary<int, FixedSizeQueue<string>> UserLogs = new();

    public static void AppendLog(IUser user, string message)
    {
        if (UserLogs.TryGetValue(user.Id, out FixedSizeQueue<string>? queue))
        {
            queue.Append(message);
        }
        else
        {
            UserLogs.TryAdd(user.Id, new FixedSizeQueue<string>(10, [message]));
        }
    }

    public static string? WriteLogs(IUser user)
    {
        if (UserLogs.TryRemove(user.Id, out FixedSizeQueue<string>? queue))
        {
            return string.Join(Environment.NewLine, queue);
        }
        else
        {
            return null;
        }
    }
}
