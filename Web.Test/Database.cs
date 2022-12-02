using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Test;

public abstract class Database
{
    protected TestContext TestContext { get; set; } = null!;

    protected CoreContext Context { get; private set; } = null!;

    [TestInitialize]
    public void InitDbConnection()
    {
        Context = CreateContext();
    }

    protected CoreContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("SqlConnectionString"))
            .Options;

        return new CoreContext(options);
    }
}
