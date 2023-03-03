namespace Web.Test.Tests.Real;

[TestClass]
public class TestDatabase : RealDatabase
{
    /// <summary>
    /// Checks if we can connect to the database.
    /// </summary>
    [TestMethod]
    public async Task Database_HasConnection()
    {
        Assert.IsTrue(await Context.Database.CanConnectAsync());
    }
}
