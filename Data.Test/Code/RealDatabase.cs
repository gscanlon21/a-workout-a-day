using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Data.Test.Code;

public abstract class RealDatabase : BaseTest
{
    [TestInitialize]
    public void InitDbConnection()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(Config.GetConnectionString("CoreContext"));

        Context = new CoreContext(optionsBuilder.Options);
    }
}
