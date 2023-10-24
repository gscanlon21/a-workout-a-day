using Core.Code;
using Core.Models.Options;
using Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Test.Unit;

public abstract class FakeDatabase
{
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    protected TestContext TestContext { get; set; } = null!;

    protected CoreContext Context { get; set; } = null!;

    protected IServiceProvider Services { get; set; } = null!;

    protected IConfigurationRoot Config { get; set; } = null!;

    [TestInitialize]
    public void InitDbConnection()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json", optional: true)
            .AddCustomEnvironmentVariables()
            .Build();

        var collection = new ServiceCollection();
        collection.AddOptions();
        collection.Configure<SiteSettings>(Config.GetSection("SiteSettings"));
        Services = collection.BuildServiceProvider();

        Context = CreateContext();
    }

    protected virtual CoreContext CreateContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<CoreContext>()
            .UseInMemoryDatabase(databaseName: "FinerFettle");

        return new CoreContext(optionsBuilder.Options);
    }
}
