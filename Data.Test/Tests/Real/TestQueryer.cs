using Core.Models.Exercise;
using Data.Data.Query;

namespace Data.Test.Tests.Real;


[TestClass]
public class TestQueryer : RealDatabase
{
#if DEBUG
    [Ignore]
#endif
    [TestMethod]
    public async Task ExerciseQueryer_WithExerciseVariationType_ReturnsCorrectExerciseVariationType()
    {
        var results = await new QueryBuilder()
            .WithExerciseFocus(ExerciseFocus.Strength)
            .Build()
            .Query(Context);

        Assert.IsTrue(results.All(vm => vm.Variation.ExerciseFocus.HasFlag(ExerciseFocus.Strength)));
    }
}