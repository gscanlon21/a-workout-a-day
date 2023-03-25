
namespace Core.Debug;

/// <summary>
/// Debug consts.
/// </summary>
public static class Consts
{
#if DEBUG
    public const bool IsDebug = true;
#else
    public const bool IsDebug = false;
#endif
}
