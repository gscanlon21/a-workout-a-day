namespace Web.Test.IntegrationTests.Real;


[TestClass]
public class TestNewsletter : RealDatabase
{
    /*
    public UserService UserService { get; private set; } = null!;

    public NewsletterController Controller { get; private set; } = null!;

    [TestInitialize]
    public void Init()
    {
        var mockSp = new Mock<IServiceProvider>();
        mockSp.Setup(m => m.GetService(typeof(CoreContext))).Returns(Context);
        var mockSs = new Mock<IServiceScope>();
        mockSs.Setup(m => m.ServiceProvider).Returns(mockSp.Object);
        var mockSsf = new Mock<IServiceScopeFactory>();
        mockSsf.Setup(m => m.CreateScope()).Returns(mockSs.Object);
        Controller = new NewsletterController(Context, new UserService(Context), mockSsf.Object);
        UserService = new UserService(Context);
    }

    [TestMethod]
    public async Task NewsletterController_GetWarmupExercises_HasAny()
    {
        var user = await Context.Users.FirstAsync(u => u.Email == "test@aworkoutaday.com");

        await Controller.AddMissingUserExerciseVariationRecords(user);

        var rotation = await UserService.GetTodaysWorkoutRotation(user);

        var warmupExercises = await Controller.GetWarmupExercises(user, rotation, string.Empty, excludeExercises: null);
        Assert.IsTrue(warmupExercises.Any());
    }
    */
}