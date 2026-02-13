using ADay.Data;
using Data.Repos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Data.Test.Code;

public abstract class RealDatabase : BaseTest
{
    [TestInitialize]
    public void InitDbConnection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(Config.GetConnectionString("CoreContext"));

        var sharedOptionsBuilder = new DbContextOptionsBuilder<SharedContext>()
            .UseNpgsql(Config.GetConnectionString("SharedContext"));

        Context = new CoreContext(optionsBuilder.Options);
        SharedContext = new SharedContext(sharedOptionsBuilder.Options);
        UserRepo = new Mock<UserRepo>(Context) { CallBase = true };
    }
}
