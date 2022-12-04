using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web.Controllers;
using Web.Data;
using Web.Entities.User;
using Web.Models.Newsletter;
using Web.Models.User;

namespace Web.Test.Tests.Real;


[TestClass]
public class TestNewsletter : RealDatabase
{
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
        Controller = new NewsletterController(Context, mockSsf.Object);
    }

    [TestMethod]
    public async Task NewsletterController_GetWarmupExercises_HasAny()
    {
        var user = new User()
        {
            Frequency = Frequency.FullBody2Day,
            StrengtheningPreference = StrengtheningPreference.Maintain
        };

        var rotation = await Controller.GetTodaysNewsletterRotation(user);

        var warmupExercises = await Controller.GetWarmupExercises(user, rotation, string.Empty);
        Assert.IsTrue(warmupExercises.Any());
    }
}