namespace Core.Consts;

/// <summary>
/// Shared user consts for Functions and Web.
/// </summary>
public class UserConsts
{
    #region Consts

    public const int DeloadAfterEveryXWeeksMin = 2;
    public const int DeloadAfterEveryXWeeksDefault = 10;
    public const int DeloadAfterEveryXWeeksMax = 18;

    public const int RefreshFunctionalEveryXWeeksMin = 0;
    public const int RefreshFunctionalEveryXWeeksDefault = 4;
    public const int RefreshFunctionalEveryXWeeksMax = 12;

    public const int RefreshAccessoryEveryXWeeksMin = 0;
    public const int RefreshAccessoryEveryXWeeksDefault = 1;
    public const int RefreshAccessoryEveryXWeeksMax = 12;

    public const int UserMuscleMobilityMin = 0;
    public const int UserMuscleMobilityMax = 2;

    #endregion

    /// <summary>
    /// The lowest the user's progression can go.
    /// 
    /// Also the user's starting progression when the user is new to fitness.
    /// </summary>
    public const int MinUserProgression = 5;

    /// <summary>
    /// The highest the user's progression can go.
    /// </summary>
    public const int MaxUserProgression = 95;

    /// <summary>
    /// How many custom user_frequency records do we allow per user?
    /// </summary>
    public const int MaxUserFrequencies = 7;

    /// <summary>
    /// How much to increment the user_muscle_strength target ranges with each increment?
    /// </summary>
    public const int IncrementMuscleTargetBy = 10;

    /// <summary>
    /// How many months until the user's account is disabled for inactivity.
    /// </summary>
    public const int DisableAfterXMonths = 3;

    /// <summary>
    /// How many months until the user's account is deleted for inactivity.
    /// </summary>
    public const int DeleteAfterXMonths = 6;

    /// <summary>
    /// How many months until the user's newsletter logs are deleted.
    /// </summary>
    public const int DeleteLogsAfterXMonths = 12;
}
