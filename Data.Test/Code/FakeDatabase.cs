using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Data.Test.Code;

public abstract class FakeDatabase : BaseTest
{
    [TestInitialize]
    public void InitDbConnection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString());

        Context = new CoreContext(optionsBuilder.Options);
        UserRepo = new Mock<UserRepo>(Context) { CallBase = true };
    }
}
