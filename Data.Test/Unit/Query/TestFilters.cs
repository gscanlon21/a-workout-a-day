using Core.Code.Extensions;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Core.Models.User;
using Data.Entities.Exercise;
using Data.Query;
using Data.Test.Code;
using Microsoft.EntityFrameworkCore;

namespace Data.Test.Unit.Query;

[TestClass]
public class TestFilters : RealDatabase
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
    public async Task FilterExerciseType_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetValuesExcluding32(Section.None, Section.Debug))
        {
            var results = Filters.FilterSection(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.Section.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterEquipment_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<Equipment>())
        {
            var results = Filters.FilterEquipment(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.Instructions.All(i => i.Equipment.HasFlag(filter))));
        }
    }

    [TestMethod]
    public async Task FilterExerciseFocus_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<ExerciseFocus>())
        {
            var results = Filters.FilterExerciseFocus(ExerciseVariationsQuery!, [filter]).ToList();
            Assert.IsTrue(results.All(r => r.Variation.ExerciseFocus.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterJoints_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<Joints>())
        {
            var results = Filters.FilterJoints(ExerciseVariationsQuery!, filter, include: true).ToList();
            Assert.IsTrue(results.All(r => r.Variation.MobilityJoints.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterMovementPatterns_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<MovementPattern>())
        {
            var results = Filters.FilterMovementPattern(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.MovementPattern.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterMuscleGroup_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<MuscleGroups>())
        {
            var results = Filters.FilterMuscleGroup(ExerciseVariationsQuery!, filter, include: true, (vm) => vm.Variation.StrengthMuscles).ToList();
            Assert.IsTrue(results.All(r => r.Variation.StrengthMuscles.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterMuscleMovement_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<MuscleMovement>())
        {
            var results = Filters.FilterMuscleMovement(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.MuscleMovement.HasAnyFlag32(filter)));
        }
    }

    [TestMethod]
    public async Task FilterSportsFocus_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues32<SportsFocus>())
        {
            var results = Filters.FilterSportsFocus(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.SportsFocus.HasAnyFlag32(filter)));
        }
    }
}