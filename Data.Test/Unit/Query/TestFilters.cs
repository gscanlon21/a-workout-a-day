using Core.Code.Extensions;
using Core.Models.Equipment;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Exercise;
using Data.Query;
using Data.Test.Code;
using Data.Test.Code.Attributes;
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

    [TestMethodOnRemote]
    public async Task FilterExerciseType_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetValuesExcluding(Section.None, Section.Debug))
        {
            var results = Filters.FilterSection(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.Section.HasAnyFlag(filter)));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterEquipment_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<Equipment>())
        {
            var results = Filters.FilterEquipment(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.Instructions.All(i => i.Equipment.HasFlag(filter))));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterExerciseFocus_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<ExerciseFocus>())
        {
            var results = Filters.FilterExerciseFocus(ExerciseVariationsQuery!, [filter]).ToList();
            Assert.IsTrue(results.All(r => r.Variation.ExerciseFocus.HasAnyFlag(filter)));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterMovementPatterns_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<MovementPattern>())
        {
            var results = Filters.FilterMovementPattern(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.MovementPattern.HasAnyFlag(filter)));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterMuscleGroup_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<MusculoskeletalSystem>())
        {
            var results = Filters.FilterMuscleGroup(ExerciseVariationsQuery!, filter, include: true, (vm) => vm.Variation.Strengthens).ToList();
            Assert.IsTrue(results.All(r => r.Variation.Strengthens.HasAnyFlag(filter)));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterMuscleMovement_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<MuscleMovement>())
        {
            var results = Filters.FilterMuscleMovement(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.MuscleMovement.HasAnyFlag(filter)));
        }
    }

    [TestMethodOnRemote]
    public async Task FilterSportsFocus_ReturnsFiltered()
    {
        foreach (var filter in EnumExtensions.GetNotNoneValues<SportsFocus>())
        {
            var results = Filters.FilterSportsFocus(ExerciseVariationsQuery!, filter).ToList();
            Assert.IsTrue(results.All(r => r.Variation.SportsFocus.HasAnyFlag(filter)));
        }
    }
}