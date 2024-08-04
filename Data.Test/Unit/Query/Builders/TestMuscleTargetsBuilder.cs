using Data.Entities.Exercise;
using Data.Entities.User;
using Data.Query;
using Data.Query.Builders;
using Data.Test.Code;
using Microsoft.EntityFrameworkCore;

namespace Data.Test.Unit.Query.Builders;


[TestClass]
public class TestMuscleTargetsBuilder : RealDatabase
{
    private class TestExerciseVariationCombo : IExerciseVariationCombo
    {
        public Exercise Exercise { get; init; } = null!;
        public Variation Variation { get; init; } = null!;
    }

    [TestInitialize]
    public async Task Init()
    {
        ExerciseVariationsQuery = (await Context.Variations
            .AsNoTracking()
            .Include(v => v.Exercise)
            .Select(v => new TestExerciseVariationCombo()
            {
                Variation = v,
                Exercise = v.Exercise
            })
            .ToListAsync())
            .AsQueryable();
    }

    private IQueryable<IExerciseVariationCombo>? ExerciseVariationsQuery { get; set; } = null!;

    [TestMethod]
    public async Task AdjustCoreMuscles()
    {
        var builder = MuscleTargetsBuilder
                .WithMuscleGroups(UserMuscleMobility.MuscleTargets.Select(mt => mt.Key).ToList())
                .WithoutMuscleTargets();
        Assert.IsTrue(true);
    }
}