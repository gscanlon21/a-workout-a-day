namespace Core.Consts;

/// <summary>
/// Debug consts. Useful for attribute parameters.
/// </summary>
public static class DebugConsts
{
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
