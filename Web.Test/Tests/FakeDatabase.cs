using Microsoft.EntityFrameworkCore;
using Web.Data;

namespace Web.Test.Tests;

public abstract class FakeDatabase
{
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    protected TestContext TestContext { get; set; } = null!;

    protected CoreContext Context { get; set; } = null!;

    [TestInitialize]
    public void InitDbConnection()
    {
        Context = CreateContext();
    }

    protected virtual CoreContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: "FinerFettle");

        return new CoreContext(optionsBuilder.Options);
    }
}
