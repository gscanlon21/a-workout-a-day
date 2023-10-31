using Microsoft.EntityFrameworkCore;

namespace Data.Test.Code;

public abstract class FakeDatabase : BaseTest
{
    [TestInitialize]
    public void InitDbConnection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

        Context = new CoreContext(optionsBuilder.Options);
    }
}
