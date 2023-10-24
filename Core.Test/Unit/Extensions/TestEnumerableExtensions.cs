using Core.Code.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Core.Test.Unit.Extensions;

[TestClass]

public class TestEnumerableExtensions
{
    [TestMethod]
    public async Task NullIfEmpty_ReturnsNotNullIfNotEmpty()
    {
        var source = Enumerable.Repeat(1, 10);
        Assert.IsNotNull(source.NullIfEmpty());
    }

    [TestMethod]
    public async Task NullIfEmpty_ReturnsNullIfEmpty()
    {
        var source = Enumerable.Empty<int>();
        Assert.IsNull(source.NullIfEmpty());
    }
}
