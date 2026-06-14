using Core.Dtos.Exercise;
using Core.Models.Exercise;
using Core.Models.Newsletter;
using Data.Entities.Exercises;
using Data.Query.Options;
using Microsoft.Extensions.DependencyInjection;
using static Core.Code.Extensions.EnumerableExtensions;

namespace Data.Query.Runners;

/// <summary>
/// Builds and runs an EF Core query for selecting exercises.
/// </summary>
public class SystemQueryRunner : BaseQueryRunner
{
    public SystemQueryRunner(Section section) : base(section) { }

    protected override IQueryable<ExercisesQueryResults> Map(IQueryable<Exercise> exercises, bool includePrerequisites)
    {
        if (includePrerequisites)
        {
            return exercises.Select(e => new ExercisesQueryResults()
            {
                Exercise = e,
                UserExercise = null!,
                // Pull these out of the constructor so EF Core can filter out unused properties.
                Prerequisites = e.Prerequisites.Where(p => p.PrerequisiteExercise.DisabledReason == null).Select(p => new ExercisePrerequisiteDto()
                {
                    Proficiency = p.Proficiency,
                    Id = p.PrerequisiteExerciseId,
                    Name = p.PrerequisiteExercise.Name,
                }).ToList(),
                Postrequisites = e.Postrequisites.Where(p => p.Exercise.DisabledReason == null).Select(p => new ExercisePrerequisiteDto()
                {
                    Id = p.ExerciseId,
                    Name = p.Exercise.Name,
                    Proficiency = p.Proficiency,
                }).ToList()
            });
        }

        return exercises.Select(e => new ExercisesQueryResults()
        {
            Exercise = e,
            UserExercise = null!,
        });
    }

    protected override IQueryable<VariationsQueryResults> Map(IQueryable<Variation> variations)
    {
        return variations.Select(v => new VariationsQueryResults()
        {
            Variation = v,
            UserVariation = null!,
        });
    }

    protected override IQueryable<ExerciseVariationsQueryResults> Map(IQueryable<ExerciseVariation> exerciseVariations)
    {
        return exerciseVariations.Select(ev => new ExerciseVariationsQueryResults()
        {
            Exercise = ev.Exercise,
            Variation = ev.Variation,
            UserExercise = ev.UserExercise,
            UserVariation = ev.UserVariation,
            Prerequisites = ev.Prerequisites,
            Postrequisites = ev.Postrequisites,
            IsMinProgressionInRange = true,
            IsMaxProgressionInRange = true,
            UserOwnsEquipment = true,
        });
    }

    protected override IQueryable<ExerciseVariationsQueryResults> Filter(IQueryable<ExerciseVariationsQueryResults> exerciseVariations, bool ignoreExclusions = false)
    {
        return base.Filter(exerciseVariations, ignoreExclusions: ignoreExclusions);
    }

    /// <summary>
    /// Queries the db for the data.
    /// </summary>
    /// <param name="take">Selects this many variations.</param>
    public override async Task<IList<QueryResults>> Query(IServiceScopeFactory factory, OrderBy orderBy = OrderBy.None, int take = int.MaxValue)
    {
        // Short-circut when either of these options are set without any data. No results are returned.
        if (ExerciseOptions.ExerciseIds?.Any() == false || ExerciseOptions.VariationIds?.Any() == false)
        {
            return [];
        }

        using var scope = factory.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<CoreContext>();

        var queryResults = await QueryPartial(context);
        return queryResults.Select(r => new QueryResults(section, r.Exercise, r.Variation, r.UserExercise, r.UserVariation, r.Prerequisites, r.Postrequisites, r.EasierVariation, r.HarderVariation, Intensity.None))
            .OrderBy(vm => vm.Variation.Progression.Min, NullOrder.NullsFirst)
            .ThenBy(vm => vm.Variation.Progression.Max, NullOrder.NullsLast)
            .ThenBy(vm => vm.Variation.Name)
            .ToList();
    }
}
