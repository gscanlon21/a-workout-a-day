using Core.Code.Helpers;
using Core.Dtos.Newsletter;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Query;
using Data.Query.Builders;
using Data.Repos;
using Data.Test.Code;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace Data.Test.Unit.Query.Builders;

[TestClass]
public class TestMuscleTargetsBuilder : RealDatabase
{
    private NewsletterRepo NewsletterRepo { get; set; } = null!;

    private class TestExerciseVariationCombo : IExerciseVariationCombo
    {
        public Exercise Exercise { get; init; } = null!;
        public Variation Variation { get; init; } = null!;
    }

    [TestInitialize]
    public async Task Init()
    {
        var mockSp = new Mock<IServiceProvider>();
        mockSp.Setup(m => m.GetService(typeof(CoreContext))).Returns(Context);
        var mockSs = new Mock<IServiceScope>();
        mockSs.Setup(m => m.ServiceProvider).Returns(mockSp.Object);
        var mockSsf = new Mock<IServiceScopeFactory>();
        mockSsf.Setup(m => m.CreateScope()).Returns(mockSs.Object);
        var mockLoggerNewsletterRepo = new Mock<ILogger<NewsletterRepo>>();

        NewsletterRepo = new NewsletterRepo(mockLoggerNewsletterRepo.Object, Context, UserRepo.Object, mockSsf.Object);
    }

    [TestMethod]
    public async Task AdjustCoreMuscles()
    {
        var muscleGroups = UserMuscleMobility.MuscleTargets.Select(mt => mt.Key).ToList();
        var builder = MuscleTargetsBuilder
                .WithMuscleGroups(muscleGroups)
                .WithoutMuscleTargets()
                .Build(Section.None);

        Assert.IsTrue(builder.MuscleGroups.SequenceEqual(muscleGroups));
    }

    [TestMethod]
    public async Task AdjustCoreMuscles2()
    {
        var user = await UserRepo.Object.GetUserStrict(UserConsts.DemoUser, UserConsts.DemoToken, allowDemoUser: true);
        UserRepo.Setup(m => m.GetNextRotation(It.IsAny<User>())).ReturnsAsync((Frequency.PushPullLeg3Day, new WorkoutRotationDto()));
        var context = await NewsletterRepo.BuildWorkoutContext(user, UserConsts.DemoToken, DateHelpers.Today);
        var muscleGroups = UserMuscleMobility.MuscleTargets.Select(mt => mt.Key).ToList();
        var builder = MuscleTargetsBuilder
                .WithMuscleGroups(context!, muscleGroups)
                .WithMuscleTargetsFromMuscleGroups()
                .AdjustMuscleTargets()
                .Build(Section.None);

        Assert.IsTrue(builder.MuscleGroups.SequenceEqual(muscleGroups));
    }
}