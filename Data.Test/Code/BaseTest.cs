using Core.Code;
using Core.Models.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Data.Test.Code;

public abstract class BaseTest
{
    protected static DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);

    protected TestContext TestContext { get; set; } = null!;

    protected CoreContext Context { get; set; } = null!;

    protected IServiceProvider Services { get; set; } = null!;

    protected IConfigurationRoot Config { get; set; } = null!;

    [TestInitialize]
    public void InitConfig()
    {
        Config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("test.appsettings.json", optional: true)
            .AddCustomEnvironmentVariables()
            .Build();

        var collection = new ServiceCollection();
        collection.AddOptions();
        collection.Configure<SiteSettings>(Config.GetSection("SiteSettings"));
        Services = collection.BuildServiceProvider();
    }
}
