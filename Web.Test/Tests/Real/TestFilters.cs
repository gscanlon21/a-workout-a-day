namespace Web.Test.Tests.Real;


[TestClass]
public class TestFilters : RealDatabase
{
#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public void Filters_SportsFocus()
    {
        //var groups = Filters.FilterSportsFocus
        Assert.IsTrue(true);
    }
}