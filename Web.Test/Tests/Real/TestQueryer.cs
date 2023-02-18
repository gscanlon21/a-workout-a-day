using Web.Models.Exercise;

namespace Web.Test.Tests.Real;


[TestClass]
public class TestQueryer : RealDatabase
{
    [TestMethod]
    public async Task ExerciseQueryer_WithExerciseType_ReturnsCorrectExerciseType()
    {
        var results = await new ExerciseQueryBuilder(Context)
            .WithExerciseType(ExerciseType.Main)
            .Build()
            .Query();

        Assert.IsTrue(results.All(vm => vm.ExerciseVariation.ExerciseType.HasFlag(ExerciseType.Main)));
    }
}