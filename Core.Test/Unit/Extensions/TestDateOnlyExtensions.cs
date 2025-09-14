using Core.Code.Extensions;
using System.Globalization;

namespace Core.Test.Unit.Extensions;

/// <summary>
/// 2024-08-04 = Sunday
/// 2024-08-11 = Sunday
/// 2024-08-18 = Sunday
/// 2024-08-25 = Sunday
/// 
/// 2024-08-05 = Monday
/// 2024-08-12 = Monday
/// 2024-08-19 = Monday
/// 2024-08-26 = Monday
/// 
/// 2024-08-02 = Friday
/// 2024-08-09 = Friday
/// 2024-08-16 = Friday
/// 2024-08-23 = Friday
/// 2024-08-30 = Friday
/// 
/// 2024-08-03 = Saturday
/// 2024-08-10 = Saturday
/// 2024-08-17 = Saturday
/// 2024-08-24 = Saturday
/// 2024-08-31 = Saturday
/// </summary>
[TestClass]
public class TestDateOnlyExtensions
{
    [TestMethod]
    public async Task StartOfWeek_IsSunday_WhenMonday()
    {
        var sunday = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        var monday = DateOnly.Parse("2024-08-05", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, monday.StartOfWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task StartOfWeek_IsSunday_WhenSunday()
    {
        var sunday = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        var sunday2 = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, sunday2.StartOfWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task EndOfWeek_IsSaturday_WhenFriday()
    {
        var saturday = DateOnly.Parse("2024-08-10", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-09", CultureInfo.InvariantCulture);
        Assert.AreEqual(saturday, friday.EndOfWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task StartOfNextWeek_IsSunday_WhenFriday()
    {
        var sunday = DateOnly.Parse("2024-08-11", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-09", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, friday.StartOfNextWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task EndOfNextWeek_IsSaturday_WhenFriday()
    {
        var sunday = DateOnly.Parse("2024-08-17", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-09", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, friday.EndOfNextWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task StartOfPreviousWeek_IsSunday_WhenFriday()
    {
        var sunday = DateOnly.Parse("2024-08-11", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-23", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, friday.StartOfPreviousWeek(DayOfWeek.Sunday));
    }

    [TestMethod]
    public async Task EndOfPreviousWeek_IsSaturday_WhenFriday()
    {
        var sunday = DateOnly.Parse("2024-08-17", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-23", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, friday.EndOfPreviousWeek(DayOfWeek.Sunday));
    }
}
