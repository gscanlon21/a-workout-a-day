using Core.Code.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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