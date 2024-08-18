
namespace Core.Consts;

/// <summary>
/// Shared user consts for Functions and Web.
/// </summary>
public class EmailConsts
{
    public const string SubjectLogin = "Account Access";

    public const string SubjectConfirm = "Account Confirmation";

    public const string SubjectWorkout = "Daily Workout";

    public const string SubjectException = "Unhandled Exception";

    public const int MaxSendAttempts = 1;

    /// <summary>
    /// How many months until email logs are deleted.
    /// </summary>
    public const int DeleteEmailsAfterXMonths = 1;
}
