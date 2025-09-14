using Core.Code.Extensions;

namespace Core.Test.Unit.Extensions;

[TestClass]
public class TestRangeExtensions
{
    [TestMethod]
    public async Task GetMiddle_ReturnsMiddle()
    {
        Assert.AreEqual(62.5, new Range(25, 100).GetMiddle());
    }
}