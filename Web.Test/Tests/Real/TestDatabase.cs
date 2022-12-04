namespace Web.Test.Tests.Real;

[TestClass]
public class TestDatabase : RealDatabase
{
    [TestMethod]
    public async Task Database_HasConnection()
    {
        Assert.IsTrue(await Context.Database.CanConnectAsync());
    }
}
