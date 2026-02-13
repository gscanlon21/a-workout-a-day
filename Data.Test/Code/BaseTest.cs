using ADay.Data;
using Core.Models.Options;
using Data.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Data.Test.Code;

public abstract class BaseTest
{
    protected TestContext TestContext { get; set; } = null!;

    protected CoreContext Context { get; set; } = null!;

    protected SharedContext SharedContext { get; set; } = null!;

    protected Mock<UserRepo> UserRepo { get; set; } = null!;

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
