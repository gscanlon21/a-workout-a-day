namespace Core.User;

/// <summary>
/// Shared user consts for Functions and Web.
/// </summary>
public class Consts
{
    /// <summary>
    /// How long until the user's account is disabled for inactivity.
    /// </summary>
    public const int DisableAfterXMonths = 3;

    /// <summary>
    /// How long until the user's account is deleted for inactivity.
    /// </summary>
    public const int DeleteAfterXMonths = 6;
}
