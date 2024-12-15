using Web.Code.Extensions;

namespace Web.Test.Unit.Extensions;

[TestClass]
public class TestEnumViewExtensions
{
    private enum ScrambledEnum
    {
        A = 14,
        C = 1,
        B = 2,
        D = 3,
        E = 0,
    }

    [TestMethod]
    public async Task AsSelectListItems32_OrderByText_ReturnsCorrectOrder()
    {
        var values = new List<ScrambledEnum>() { ScrambledEnum.C, ScrambledEnum.B, ScrambledEnum.E, ScrambledEnum.A, ScrambledEnum.D };
        var items = values.AsSelectListItems(EnumViewExtensions.EnumOrdering.Text);
        // Yes, the default value should always come first.
        Assert.IsTrue(items.Select(i => i.Text).SequenceEqual(["E", "A", "B", "C", "D"]));
    }

    [TestMethod]
    public async Task AsSelectListItems32_OrderByValue_ReturnsCorrectOrder()
    {
        var values = new List<ScrambledEnum>() { ScrambledEnum.C, ScrambledEnum.B, ScrambledEnum.E, ScrambledEnum.A, ScrambledEnum.D };
        var items = values.AsSelectListItems(EnumViewExtensions.EnumOrdering.Value);
        Assert.IsTrue(items.Select(i => i.Value).SequenceEqual(["0", "1", "2", "3", "14"]));
    }
}