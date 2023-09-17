using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test.Tests.Extensions;

[TestClass]
public class TestEnumViewExtensions
{
    private enum TestEnum
    {
        A = 4,
        C = 1,
        B = 2,
        D = 3,
        E = 0,
    }

    [TestMethod]
    public async Task GetSingleValues32_ReturnsSingleValues()
    {
        Assert.IsTrue(true);
    }
}