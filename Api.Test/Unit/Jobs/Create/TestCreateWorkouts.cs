using Api.Jobs.Create;
using Core.Code.Helpers;
using Core.Models.Options;
using Data;
using Data.Repos;
using Data.Test.Code;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Api.Test.Unit.Jobs.Create;

[TestClass]
public class TestCreateWorkouts : FakeDatabase
{
    private CreateWorkouts NewsletterJob { get; set; } = null!;

    [TestInitialize]
    public void Init()
    {
        var mockSp = new Mock<IServiceProvider>();
        mockSp.Setup(m => m.GetService(typeof(CoreContext))).Returns(Context);
        var mockSs = new Mock<IServiceScope>();
        mockSs.Setup(m => m.ServiceProvider).Returns(mockSp.Object);
        var mockSsf = new Mock<IServiceScopeFactory>();
        mockSsf.Setup(m => m.CreateScope()).Returns(mockSs.Object);

        var mockHttpClient = new Mock<HttpClient>();
        var mockHttpClientFactory = new Mock<IHttpClientFactory>();
        mockHttpClientFactory.Setup(m => m.CreateClient(It.IsAny<string>())).Returns(mockHttpClient.Object);

        var mockLoggerNewsletterJob = new Mock<ILogger<CreateWorkouts>>();
        var userRepo = new UserRepo(Context);

        NewsletterJob = new CreateWorkouts(
            mockLoggerNewsletterJob.Object,
            mockSsf.Object,
            userRepo,
            Services.GetService<IOptions<SiteSettings>>()!,
            Context
        );
    }

    [TestMethod]
    public async Task GetUsers_WhenNewsletterIsDisabled_ReturnsOne()
    {
        Context.Users.Add(new Data.Entities.User.User(string.Empty, true, false)
        {
            LastActive = DateHelpers.Today,
            NewsletterDisabledReason = "testing",
            SendDays = Core.Models.User.Days.All,
            SendHour = DateHelpers.CurrentHour,
        });
        await Context.SaveChangesAsync();

        var users = await NewsletterJob.GetUsers().ToListAsync();
        Assert.IsTrue(users.Count == 1);
    }

    [TestMethod]
    public async Task GetUsers_WhenSendDaysIsNone_ReturnsNone()
    {
        Context.Users.Add(new Data.Entities.User.User(string.Empty, true, false)
        {
            SendDays = Core.Models.User.Days.None,
            IncludeMobilityWorkouts = false,
        });
        await Context.SaveChangesAsync();

        var users = await NewsletterJob.GetUsers().ToListAsync();
        Assert.IsTrue(users.Count == 0);
    }

    [TestMethod]
    public async Task GetUsers_WhenLastActiveIsNull_ReturnsNone()
    {
        Context.Users.Add(new Data.Entities.User.User(string.Empty, true, false)
        {
            SendDays = Core.Models.User.Days.None,
            IncludeMobilityWorkouts = true,
        });
        await Context.SaveChangesAsync();

        var users = await NewsletterJob.GetUsers().ToListAsync();
        Assert.IsTrue(users.Count == 0);
    }

    [TestMethod]
    public async Task GetUsers_WhenIncludeMobilityWorkouts_ReturnsNone()
    {
        Context.Users.Add(new Data.Entities.User.User(string.Empty, true, false)
        {
            LastActive = DateHelpers.Today,
            IncludeMobilityWorkouts = true,
            SendDays = Core.Models.User.Days.None,
            SendHour = DateHelpers.CurrentHour,
        });
        await Context.SaveChangesAsync();

        var users = await NewsletterJob.GetUsers().ToListAsync();
        Assert.IsTrue(users.Count == 0);
    }

    [TestMethod]
    public async Task GetUsers_WhenActive_ReturnsNone()
    {
        Context.Users.Add(new Data.Entities.User.User(string.Empty, true, false)
        {
            LastActive = DateHelpers.Today,
            SendDays = Core.Models.User.Days.All,
            SendHour = DateHelpers.CurrentHour,
        });
        await Context.SaveChangesAsync();

        var users = await NewsletterJob.GetUsers().ToListAsync();
        Assert.IsTrue(users.Count == 0);
    }
}
