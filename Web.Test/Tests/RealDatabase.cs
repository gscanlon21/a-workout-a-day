using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Test.Tests;

public abstract class RealDatabase : FakeDatabase
{
    protected override CoreContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseNpgsql(Environment.GetEnvironmentVariable("SqlConnectionString"));

        return new CoreContext(optionsBuilder.Options);
    }
}
