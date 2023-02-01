using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using Web.Controllers;
using Web.Data;
using Web.Entities.User;
using Web.Models.Newsletter;
using Web.Models.User;
using Web.Services;

namespace Web.Test.Tests.Fake;


[TestClass]
public class TestNewsletter : FakeDatabase
{
    public UserService UserService { get; private set; } = null!;
    public NewsletterController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Init()
    {
        var mockSp = new Mock<IServiceProvider>();
        mockSp.Setup(m => m.GetService(typeof(CoreContext))).Returns(CreateContext());
        var mockSs = new Mock<IServiceScope>();
        mockSs.Setup(m => m.ServiceProvider).Returns(mockSp.Object);
        var mockSsf = new Mock<IServiceScopeFactory>();
        mockSsf.Setup(m => m.CreateScope()).Returns(mockSs.Object);
        UserService = new UserService(Context);
        Controller = new NewsletterController(Context, UserService, mockSsf.Object);
    }

    [TestMethod]
    public async Task NewsletterController_CheckNewsletterDeloadStatus_HasNoTimeWhenIsDeloadWeek()
    {
        // Insert seed data into the database using one instance of the context
        var user = new User("test@test.finerfettle.com", true, false)
        {
            Frequency = Frequency.FullBody2Day,
            StrengtheningPreference = StrengtheningPreference.Light,
            DeloadAfterEveryXWeeks = 2
        };

        Context.Newsletters.Add(new Entities.Newsletter.Newsletter(Today.AddDays(user.DeloadAfterEveryXWeeks * -7 - 1), user, await Controller.GetTodaysNewsletterRotation(user), isDeloadWeek: true));
        for (int i = user.DeloadAfterEveryXWeeks * 7; i > 0; i--)
        {
            var rotation = await Controller.GetTodaysNewsletterRotation(user);
            var (needsDeload, timeUntilDeload) = await UserService.CheckNewsletterDeloadStatus(user);
            Context.Newsletters.Add(new Entities.Newsletter.Newsletter(Today.AddDays(-1 * i), user, rotation, isDeloadWeek: needsDeload));
        }
        
        Context.SaveChanges();

        Assert.AreEqual((true, TimeSpan.Zero), await UserService.CheckNewsletterDeloadStatus(user));
    }
}