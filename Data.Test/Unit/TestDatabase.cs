
using Data.Test.Code;
using Data.Test.Code.Attributes;

namespace Data.Test.Unit;

[TestClass]
public class TestDatabase : RealDatabase
{
    /// <summary>
    /// Checks if we can connect to the database.
    /// </summary>
    [TestMethodOnRemote]
    public async Task Database_HasConnection()
    {
        Assert.IsTrue(await Context.Database.CanConnectAsync());
    }
}
