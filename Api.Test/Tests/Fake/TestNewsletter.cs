namespace Api.Test.Tests.Fake;


[TestClass]
public class TestNewsletter : FakeDatabase
{
    /*
    public UserCon UserService { get; private set; } = null!;
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
        var user = new User("test@aworkoutaday.com", true, false)
        {
            Frequency = Frequency.FullBody2Day,
            Intensity = Intensity.Light,
            DeloadAfterEveryXWeeks = 2
        };

        Context.Newsletters.Add(new Entities.Newsletter.Newsletter(Today.AddDays((user.DeloadAfterEveryXWeeks * -7) - 1), user, await UserService.GetTodaysWorkoutRotation(user), user.Frequency, isDeloadWeek: true));
        for (int i = user.DeloadAfterEveryXWeeks * 7; i > 0; i--)
        {
            var rotation = await UserService.GetTodaysWorkoutRotation(user);
            var (needsDeload, timeUntilDeload) = await UserService.CheckNewsletterDeloadStatus(user);
            Context.Newsletters.Add(new Entities.Newsletter.Newsletter(Today.AddDays(-1 * i), user, rotation, user.Frequency, isDeloadWeek: needsDeload));
        }

        Context.SaveChanges();

        Assert.IsTrue((await UserService.CheckNewsletterDeloadStatus(user)).needsDeload);
    }
    */
}