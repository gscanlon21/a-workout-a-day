
namespace Web.Test.Tests;

[TestClass]
public class TestDatabase : Database
{
    [TestMethod]
    public async Task Database_HasConnection()
    {
        Assert.IsTrue(await Context.Database.CanConnectAsync());
    }
}
