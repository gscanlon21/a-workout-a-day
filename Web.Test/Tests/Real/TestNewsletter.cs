using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Web.Code.Extensions;
using Web.Controllers.Newsletter;
using Web.Data;
using Web.Entities.User;

namespace Web.Test.Tests.Real;


[TestClass]
public class TestNewsletter : RealDatabase
{
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
        Controller = new NewsletterController(Context, new Services.UserService(Context), mockSsf.Object);
    }

    [TestMethod]
    public async Task NewsletterController_GetWarmupExercises_HasAny()
    {
        var user = await Context.Users.FirstAsync(u => u.Email == "test@test.finerfettle.com");

        // The exercise queryer requires UserExercise/UserExerciseVariation/UserVariation records to have already been made
        Context.AddMissing(await Context.UserExercises.Where(ue => ue.User == user).Select(ue => ue.ExerciseId).ToListAsync(),
            await Context.Exercises.Select(e => new { e.Id, e.Proficiency }).ToListAsync(), k => k.Id, e => new UserExercise() { ExerciseId = e.Id, UserId = user.Id, Progression = user.IsNewToFitness ? UserExercise.MinUserProgression : e.Proficiency });
        Context.AddMissing(await Context.UserExerciseVariations.Where(ue => ue.User == user).Select(uev => uev.ExerciseVariationId).ToListAsync(),
            await Context.ExerciseVariations.Select(ev => ev.Id).ToListAsync(), evId => new UserExerciseVariation() { ExerciseVariationId = evId, UserId = user.Id });
        Context.AddMissing(await Context.UserVariations.Where(ue => ue.User == user).Select(uv => uv.VariationId).ToListAsync(),
            await Context.Variations.Select(v => v.Id).ToListAsync(), vId => new UserVariation() { VariationId = vId, UserId = user.Id });

        await Context.SaveChangesAsync();

        var rotation = await Controller.GetTodaysNewsletterRotation(user);

        var warmupExercises = await Controller.GetWarmupExercises(user, rotation, string.Empty, excludeExercises: null);
        Assert.IsTrue(warmupExercises.Any());
    }
}