
namespace Functions.Code;

/// <summary>
/// Debug consts.
/// </summary>
public static class Debug
{
#if DEBUG
    public const bool RunOnStartup = true;
#else
    public const bool RunOnStartup = false;
#endif
}
