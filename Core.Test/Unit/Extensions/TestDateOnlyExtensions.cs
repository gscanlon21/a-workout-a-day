using Core.Code.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;

namespace Core.Test.Unit.Extensions;

[TestClass]
public class TestDateOnlyExtensions
{
    [TestMethod]
    public async Task StartOfWeek_IsSunday_WhenMonday()
    {
        var sunday = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        var monday = DateOnly.Parse("2024-08-05", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, monday.StartOfWeek());
    }

    [TestMethod]
    public async Task StartOfWeek_IsSunday_WhenSunday()
    {
        var sunday = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        var sunday2 = DateOnly.Parse("2024-08-04", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, sunday2.StartOfWeek());
    }

    [TestMethod]
    public async Task EndOfWeek_IsSaturday_WhenFriday()
    {
        var saturday = DateOnly.Parse("2024-08-10", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-09", CultureInfo.InvariantCulture);
        Assert.AreEqual(saturday, friday.EndOfWeek());
    }

    [TestMethod]
    public async Task StartOfNextWeek_IsSunday_WhenFriday()
    {
        var sunday = DateOnly.Parse("2024-08-11", CultureInfo.InvariantCulture);
        var friday = DateOnly.Parse("2024-08-09", CultureInfo.InvariantCulture);
        Assert.AreEqual(sunday, friday.StartOfNextWeek());
    }
}
