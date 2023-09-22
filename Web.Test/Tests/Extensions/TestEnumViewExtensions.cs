using Web.Code.Extensions;

namespace Web.Test.Tests.Extensions;

[TestClass]
public class TestEnumViewExtensions
{
    private enum ScrambledEnum
    {
        A = 4,
        C = 1,
        B = 2,
        D = 3,
        E = 0,
    }

    [TestMethod]
    public async Task AsSelectListItems32_OrderByText_ReturnsCorrectOrder()
    {
        var values = new List<ScrambledEnum>() { ScrambledEnum.C, ScrambledEnum.B, ScrambledEnum.E, ScrambledEnum.A, ScrambledEnum.D };
        var items = values.AsSelectListItems32(EnumViewExtensions.EnumOrdering.Text);
        // Yes, the default value should always come first.
        Assert.IsTrue(items.Select(i => i.Text).SequenceEqual(new List<string>() { "E", "A", "B", "C", "D" }));
    }

    [TestMethod]
    public async Task AsSelectListItems32_OrderByValue_ReturnsCorrectOrder()
    {
        var values = new List<ScrambledEnum>() { ScrambledEnum.C, ScrambledEnum.B, ScrambledEnum.E, ScrambledEnum.A, ScrambledEnum.D };
        var items = values.AsSelectListItems32(EnumViewExtensions.EnumOrdering.Value);
        Assert.IsTrue(items.Select(i => i.Value).SequenceEqual(new List<string>() { "0", "1", "2", "3", "4" }));
    }
}