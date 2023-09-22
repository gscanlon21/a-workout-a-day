using Core.Code.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test.Tests.Extensions;

[TestClass]
public class TestEnumViewExtensions
{
    private enum TestEnum
    {
        A = 0,
        B = 1,
        C = 2,
        D = 3,
        E = 4,
    }

    [Flags]
    private enum TestEnumFlags
    {
        A = 0,
        B = 1 << 0,
        C = 1 << 1,
        D = 1 << 2,
        E = 1 << 3,
        BE = B | E
    }

    [TestMethod]
    public async Task GetSingleValues32_ReturnsSingleValues()
    {
        var expected = new TestEnumFlags[] { TestEnumFlags.B, TestEnumFlags.C, TestEnumFlags.D, TestEnumFlags.E };
        Assert.IsTrue(expected.SequenceEqual(EnumExtensions.GetSingleValues32<TestEnumFlags>().OrderBy(e => e)));
    }
}