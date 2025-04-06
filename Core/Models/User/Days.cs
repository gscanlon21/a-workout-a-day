using System.ComponentModel.DataAnnotations;

namespace Core.Models.User;

/// <summary>
/// Enum of days of the week.
/// </summary>
[Flags]
public enum Days
{
    [Display(Name = "None")]
    None = 0,

    [Display(Name = "Monday", ShortName = "Mon")]
    Monday = 1 << 0, // 1

    [Display(Name = "Tuesday", ShortName = "Tue")]
    Tuesday = 1 << 1, // 2

    [Display(Name = "Wednesday", ShortName = "Wed")]
    Wednesday = 1 << 2, // 4

    [Display(Name = "Thursday", ShortName = "Thu")]
    Thursday = 1 << 3, // 8

    [Display(Name = "Friday", ShortName = "Fri")]
    Friday = 1 << 4, // 16

    [Display(Name = "Saturday", ShortName = "Sat")]
    Saturday = 1 << 5, // 32

    [Display(Name = "Sunday", ShortName = "Sun")]
    Sunday = 1 << 6, // 64

    [Display(Name = "All")]
    All = Monday | Tuesday | Wednesday | Thursday | Friday | Saturday | Sunday,
}

public static class DaysExtensions
{
    /// <summary>
    /// Maps the date's day of the week to the RestDays enum.
    /// </summary>
    public static Days FromDate(DateOnly date)
    {
        return Enum.GetValues<Days>().First(d => d.ToString() == date.DayOfWeek.ToString());
    }
}
